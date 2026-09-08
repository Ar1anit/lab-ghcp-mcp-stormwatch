using Microsoft.Extensions.AI;
using System.Text;

namespace StormWatch.Chat;

public sealed record KnowledgePassage(
    string Id,
    string Title,
    string Content,
    string SourcePath);

public interface IKnowledgeRetriever
{
    Task<IReadOnlyList<KnowledgePassage>> RetrieveAsync(
        string question,
        CancellationToken cancellationToken = default);
}

public sealed class EmptyKnowledgeRetriever : IKnowledgeRetriever
{
    public static EmptyKnowledgeRetriever Instance { get; } = new();

    private EmptyKnowledgeRetriever()
    {
    }

    public Task<IReadOnlyList<KnowledgePassage>> RetrieveAsync(
        string question,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<KnowledgePassage>>([]);
}

public interface ITextEmbeddingService
{
    Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateAsync(
        IReadOnlyList<string> inputs,
        CancellationToken cancellationToken = default);
}

public sealed class ModelEmbeddingService(
    IEmbeddingGenerator<string, Embedding<float>> generator) : ITextEmbeddingService
{
    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateAsync(
        IReadOnlyList<string> inputs,
        CancellationToken cancellationToken = default)
    {
        var embeddings = await generator.GenerateAsync(
            inputs,
            cancellationToken: cancellationToken);
        return embeddings.Select(embedding => embedding.Vector).ToArray();
    }
}

public sealed record LocalRagSettings(
    string DataDirectory,
    int ChunkLength,
    int ChunkOverlap,
    int ResultCount,
    float MinimumSimilarity)
{
    public const int DefaultChunkLength = 1_200;
    public const int DefaultChunkOverlap = 200;
    public const int DefaultResultCount = 3;
    public const float DefaultMinimumSimilarity = 0.25f;

    public static LocalRagSettings FromWorkingDirectory(string workingDirectory) => new(
        Path.Combine(workingDirectory, "rag-data"),
        DefaultChunkLength,
        DefaultChunkOverlap,
        DefaultResultCount,
        DefaultMinimumSimilarity);
}

public sealed class LocalVectorKnowledgeRetriever : IKnowledgeRetriever
{
    public const int MaxFileCount = 100;
    public const long MaxFileBytes = 512_000;
    public const int MaxChunkCount = 500;

    private static readonly HashSet<string> SupportedExtensions =
        new([".md", ".txt"], StringComparer.OrdinalIgnoreCase);

    private readonly IReadOnlyList<EmbeddedPassage> _index;
    private readonly ITextEmbeddingService _embeddingService;
    private readonly LocalRagSettings _settings;

    private LocalVectorKnowledgeRetriever(
        IReadOnlyList<EmbeddedPassage> index,
        ITextEmbeddingService embeddingService,
        LocalRagSettings settings)
    {
        _index = index;
        _embeddingService = embeddingService;
        _settings = settings;
    }

    public static async Task<LocalVectorKnowledgeRetriever> CreateAsync(
        LocalRagSettings settings,
        ITextEmbeddingService embeddingService,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(embeddingService);
        IReadOnlyList<KnowledgePassage> passages = await LoadPassagesAsync(
            settings,
            cancellationToken);
        IReadOnlyList<ReadOnlyMemory<float>> vectors = await embeddingService.GenerateAsync(
            passages.Select(passage => passage.Content).ToArray(),
            cancellationToken);
        if (vectors.Count != passages.Count)
        {
            throw new InvalidOperationException(
                "The embedding model returned an unexpected number of vectors.");
        }

        EmbeddedPassage[] index = passages
            .Select((passage, position) => new EmbeddedPassage(
                passage,
                Normalize(vectors[position])))
            .ToArray();
        int dimensions = index[0].Vector.Length;
        if (index.Any(item => item.Vector.Length != dimensions))
        {
            throw new InvalidOperationException(
                "The embedding model returned inconsistent vector dimensions.");
        }

        return new LocalVectorKnowledgeRetriever(index, embeddingService, settings);
    }

    public async Task<IReadOnlyList<KnowledgePassage>> RetrieveAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("A search question is required.", nameof(question));
        }

        IReadOnlyList<ReadOnlyMemory<float>> vectors = await _embeddingService.GenerateAsync(
            [question.Trim()],
            cancellationToken);
        if (vectors.Count != 1)
        {
            throw new InvalidOperationException(
                "The embedding model did not return one query vector.");
        }

        float[] queryVector = Normalize(vectors[0]);
        return _index
            .Select(item => new
            {
                item.Passage,
                Score = Similarity(queryVector, item.Vector),
            })
            .Where(result => result.Score >= _settings.MinimumSimilarity)
            .OrderByDescending(result => result.Score)
            .ThenBy(result => result.Passage.Id, StringComparer.Ordinal)
            .Take(_settings.ResultCount)
            .Select(result => result.Passage)
            .ToArray();
    }

    private static async Task<IReadOnlyList<KnowledgePassage>> LoadPassagesAsync(
        LocalRagSettings settings,
        CancellationToken cancellationToken)
    {
        ValidateSettings(settings);
        if (!Directory.Exists(settings.DataDirectory))
        {
            throw new DirectoryNotFoundException("The rag-data directory was not found.");
        }

        IReadOnlyList<string> files = EnumerateSupportedFiles(
            settings.DataDirectory,
            cancellationToken);

        var passages = new List<KnowledgePassage>();
        foreach (string file in files)
        {
            string content = await ReadBoundedUtf8Async(file, cancellationToken);
            string relativePath = Path.GetRelativePath(settings.DataDirectory, file)
                .Replace('\\', '/');
            string sourcePath =
                $"{Path.GetFileName(Path.TrimEndingDirectorySeparator(settings.DataDirectory))}/" +
                relativePath;
            int chunkNumber = 0;
            foreach (string chunk in Chunk(content, settings.ChunkLength, settings.ChunkOverlap))
            {
                chunkNumber++;
                passages.Add(new KnowledgePassage(
                    $"{sourcePath}#chunk-{chunkNumber}",
                    Path.GetFileNameWithoutExtension(file),
                    chunk,
                    sourcePath));
                if (passages.Count > MaxChunkCount)
                {
                    throw new InvalidDataException(
                        $"The local RAG corpus exceeds the {MaxChunkCount}-chunk limit.");
                }
            }
        }

        return passages.Count > 0
            ? passages
            : throw new InvalidOperationException(
                "The rag-data directory contains no usable .md or .txt content.");
    }

    private static IReadOnlyList<string> EnumerateSupportedFiles(
        string dataDirectory,
        CancellationToken cancellationToken)
    {
        string root = Path.GetFullPath(dataDirectory);
        if ((File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0)
        {
            throw new InvalidDataException("The rag-data directory cannot be a symbolic link.");
        }

        var files = new List<string>();
        var pending = new Stack<string>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string directory = pending.Pop();
            foreach (string childDirectory in Directory.EnumerateDirectories(directory))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string fullPath = EnsureWithinRoot(root, childDirectory);
                if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
                {
                    throw new InvalidDataException(
                        "The rag-data directory cannot contain symbolic links.");
                }

                pending.Push(fullPath);
            }

            foreach (string file in Directory.EnumerateFiles(directory))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string fullPath = EnsureWithinRoot(root, file);
                if ((File.GetAttributes(fullPath) & FileAttributes.ReparsePoint) != 0)
                {
                    throw new InvalidDataException(
                        "The rag-data directory cannot contain symbolic links.");
                }

                if (!SupportedExtensions.Contains(Path.GetExtension(fullPath)))
                {
                    continue;
                }

                files.Add(fullPath);
                if (files.Count > MaxFileCount)
                {
                    throw new InvalidDataException(
                        $"The rag-data directory contains more than {MaxFileCount} supported files.");
                }
            }
        }

        return files.Order(StringComparer.Ordinal).ToArray();
    }

    private static string EnsureWithinRoot(string root, string path)
    {
        string fullPath = Path.GetFullPath(path);
        string rootPrefix = Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar;
        StringComparison comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        if (!fullPath.StartsWith(rootPrefix, comparison))
        {
            throw new InvalidDataException("A RAG source resolves outside rag-data.");
        }

        return fullPath;
    }

    private static async Task<string> ReadBoundedUtf8Async(
        string path,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            8_192,
            FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var content = new MemoryStream();
        var buffer = new byte[8_192];
        while (true)
        {
            int remaining = (int)Math.Min(buffer.Length, MaxFileBytes + 1 - content.Length);
            if (remaining <= 0)
            {
                throw new InvalidDataException(
                    $"RAG source '{Path.GetFileName(path)}' exceeds the {MaxFileBytes}-byte limit.");
            }

            int read = await stream.ReadAsync(buffer.AsMemory(0, remaining), cancellationToken);
            if (read == 0)
            {
                break;
            }

            if (content.Length + read > MaxFileBytes)
            {
                throw new InvalidDataException(
                    $"RAG source '{Path.GetFileName(path)}' exceeds the {MaxFileBytes}-byte limit.");
            }

            await content.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }

        byte[] bytes = content.ToArray();
        int offset = bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf
            ? 3
            : 0;
        try
        {
            return new UTF8Encoding(false, true).GetString(bytes, offset, bytes.Length - offset);
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidDataException(
                $"RAG source '{Path.GetFileName(path)}' is not valid UTF-8.",
                exception);
        }
    }

    private static IEnumerable<string> Chunk(string content, int length, int overlap)
    {
        string normalized = content.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Trim();
        int start = 0;
        while (start < normalized.Length)
        {
            int end = Math.Min(start + length, normalized.Length);
            if (end < normalized.Length)
            {
                int split = end;
                while (split > start + (length / 2) &&
                    !char.IsWhiteSpace(normalized[split - 1]))
                {
                    split--;
                }

                if (split > start + (length / 2))
                {
                    end = split;
                }
            }

            string chunk = normalized[start..end].Trim();
            if (chunk.Length > 0)
            {
                yield return chunk;
            }

            if (end == normalized.Length)
            {
                yield break;
            }

            start = Math.Max(start + 1, end - overlap);
            while (start < end && char.IsWhiteSpace(normalized[start]))
            {
                start++;
            }
        }
    }

    private static void ValidateSettings(LocalRagSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.DataDirectory))
        {
            throw new ArgumentException("A rag-data directory is required.", nameof(settings));
        }

        if (settings.ChunkLength <= 0 ||
            settings.ChunkOverlap < 0 ||
            settings.ChunkOverlap >= settings.ChunkLength ||
            settings.ResultCount <= 0 ||
            settings.MinimumSimilarity is < -1 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(settings),
                "Local RAG chunk, result, or similarity settings are invalid.");
        }
    }

    private static float[] Normalize(ReadOnlyMemory<float> vector)
    {
        ReadOnlySpan<float> values = vector.Span;
        double magnitudeSquared = 0;
        foreach (float value in values)
        {
            if (!float.IsFinite(value))
            {
                throw new InvalidOperationException(
                    "The embedding model returned a non-finite vector.");
            }

            magnitudeSquared += value * value;
        }

        if (values.Length == 0 || magnitudeSquared <= 0 || !double.IsFinite(magnitudeSquared))
        {
            throw new InvalidOperationException("The embedding model returned an empty vector.");
        }

        double magnitude = Math.Sqrt(magnitudeSquared);
        var normalized = new float[values.Length];
        for (int index = 0; index < values.Length; index++)
        {
            normalized[index] = (float)(values[index] / magnitude);
        }

        return normalized;
    }

    private static float Similarity(float[] left, float[] right)
    {
        if (left.Length != right.Length)
        {
            throw new InvalidOperationException(
                "The embedding model returned inconsistent vector dimensions.");
        }

        float score = 0;
        for (int index = 0; index < left.Length; index++)
        {
            score += left[index] * right[index];
        }

        return score;
    }

    private sealed record EmbeddedPassage(KnowledgePassage Passage, float[] Vector);
}