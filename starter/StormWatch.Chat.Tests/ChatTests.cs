using Microsoft.Extensions.AI;

namespace StormWatch.Chat.Tests;

public sealed class ChatTests
{
    [Fact]
    public async Task ConsoleRunnerSupportsMultipleTurnsAndExit()
    {
        var session = new RecordingSession();
        var input = new StringReader("Hello\nWhat can you help with?\n/exit\n");
        var output = new StringWriter();

        int exitCode = await ConsoleChatRunner.RunAsync(
            session,
            input,
            output,
            TestContext.Current.CancellationToken);

        Assert.Equal(0, exitCode);
        Assert.Equal(["Hello", "What can you help with?"], session.Questions);
        Assert.Equal(2, CountOccurrences(output.ToString(), "StormWatch:"));
    }

    [Fact]
    public async Task LocalVectorRetrieverLoadsChunksAndRanksByCosineSimilarity()
    {
        string root = Path.Combine(Path.GetTempPath(), $"stormwatch-rag-{Guid.NewGuid():N}");
        string dataDirectory = Path.Combine(root, "rag-data");
        Directory.CreateDirectory(dataDirectory);
        try
        {
            string nestedDirectory = Path.Combine(dataDirectory, "guides");
            Directory.CreateDirectory(nestedDirectory);
            await File.WriteAllTextAsync(
                Path.Combine(nestedDirectory, "shelter.md"),
                "Storm shelter guidance\n\nMove indoors and stay away from windows.",
                TestContext.Current.CancellationToken);
            for (int index = 2; index <= 4; index++)
            {
                await File.WriteAllTextAsync(
                    Path.Combine(dataDirectory, $"shelter-{index}.txt"),
                    $"Shelter option {index}: move indoors away from windows.",
                    TestContext.Current.CancellationToken);
            }

            await File.WriteAllTextAsync(
                Path.Combine(dataDirectory, "flood.txt"),
                "Flood guidance\n\nMove to higher ground when officials advise it.",
                TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(
                Path.Combine(dataDirectory, "ignored.json"),
                "{\"content\":\"This file type is not indexed.\"}",
                TestContext.Current.CancellationToken);

            var settings = new LocalRagSettings(dataDirectory, 80, 10, 3, 0.25f);
            var embeddings = new KeywordEmbeddingService();
            LocalVectorKnowledgeRetriever retriever =
                await LocalVectorKnowledgeRetriever.CreateAsync(
                    settings,
                    embeddings,
                    TestContext.Current.CancellationToken);
            Assert.Single(embeddings.Requests);
            Assert.Equal(5, embeddings.Requests[0].Count);

            IReadOnlyList<KnowledgePassage> passages = await retriever.RetrieveAsync(
                "Where should I shelter?",
                TestContext.Current.CancellationToken);

            Assert.Equal(3, passages.Count);
            KnowledgePassage passage = passages[0];
            Assert.Equal("shelter", passage.Title);
            Assert.Equal("rag-data/guides/shelter.md", passage.SourcePath);
            Assert.True(passage.Content.Length <= settings.ChunkLength);
            Assert.Equal(2, embeddings.Requests.Count);
            Assert.Equal("Where should I shelter?", embeddings.Requests[^1].Single());
            Assert.DoesNotContain(passages, item => item.SourcePath.EndsWith(".json"));

            IReadOnlyList<KnowledgePassage> unsupported = await retriever.RetrieveAsync(
                "How do I bake bread?",
                TestContext.Current.CancellationToken);
            Assert.Empty(unsupported);
            Assert.Equal(3, embeddings.Requests.Count);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                LocalVectorKnowledgeRetriever.CreateAsync(
                    settings,
                    new ConstantEmbeddingService(
                        new ReadOnlyMemory<float>([float.NaN])),
                    TestContext.Current.CancellationToken));

            string oversizedPath = Path.Combine(dataDirectory, "zz-oversized.txt");
            await File.WriteAllBytesAsync(
                oversizedPath,
                new byte[LocalVectorKnowledgeRetriever.MaxFileBytes + 1],
                TestContext.Current.CancellationToken);
            await Assert.ThrowsAsync<InvalidDataException>(() =>
                LocalVectorKnowledgeRetriever.CreateAsync(
                    settings,
                    new ConstantEmbeddingService(
                        new ReadOnlyMemory<float>([1f, 0f, 0f])),
                    TestContext.Current.CancellationToken));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task ChatSessionRetainsHistoryWithoutRetrieval()
    {
        var completion = new RecordingCompletionService("First answer", "Second answer");
        var session = new GroundedChatSession(
            completion,
            EmptyKnowledgeRetriever.Instance,
            []);

        await session.AskAsync("First question", TestContext.Current.CancellationToken);
        await session.AskAsync("Second question", TestContext.Current.CancellationToken);

        Assert.Equal(2, completion.Requests.Count);
        Assert.Contains(
            completion.Requests[1],
            message => message.Role == ChatRole.User && message.Text == "First question");
        Assert.Contains(
            completion.Requests[1],
            message => message.Role == ChatRole.Assistant && message.Text == "First answer");
        await Assert.ThrowsAsync<ArgumentException>(() => session.AskAsync(
            new string('x', GroundedChatSession.MaxQuestionLength + 1),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GroundedSessionUsesSourcesAndRetainsConversationHistory()
    {
        var completion = new RecordingCompletionService("First answer [S1]", "Second answer [S1]");
        var retriever = new StaticKnowledgeRetriever(
        [
            new KnowledgePassage(
                "safety-1",
                "Storm shelter guidance",
                "Move indoors and stay away from windows.",
                "rag-data/storm-safety.md"),
        ]);
        var session = new GroundedChatSession(completion, retriever, []);

        string first = await session.AskAsync(
            "Where should I shelter?",
            TestContext.Current.CancellationToken);
        string second = await session.AskAsync(
            "What did I ask before?",
            TestContext.Current.CancellationToken);

        Assert.Contains("Sources:", first);
        Assert.Contains("[S1] Storm shelter guidance", first);
        Assert.Contains("rag-data/storm-safety.md", first);
        Assert.Contains("Second answer [S1]", second);
        Assert.Equal(2, completion.Requests.Count);
        Assert.Contains(
            completion.Requests[1],
            message => message.Role == ChatRole.User &&
                message.Text.Contains("Where should I shelter?", StringComparison.Ordinal));
        Assert.Contains(
            completion.Requests[1],
            message => message.Role == ChatRole.Assistant &&
                message.Text.Contains("First answer [S1]", StringComparison.Ordinal));
    }

    [Fact]
    public void LocalRagSettingsUseRagDataFolderAndOptionsIsolateSecrets()
    {
        string workingDirectory = Path.Combine(Path.GetTempPath(), "stormwatch-options");
        LocalRagSettings settings = LocalRagSettings.FromWorkingDirectory(workingDirectory);
        Assert.Equal(Path.Combine(workingDirectory, "rag-data"), settings.DataDirectory);
        Assert.Equal(3, settings.ResultCount);
        Assert.True(settings.ChunkOverlap < settings.ChunkLength);

        ChatRuntimeOptions options = ChatRuntimeOptions.Parse(
            [
                "--with-tools",
                "--server-project",
                "reference.csproj",
                "--fixture",
                "forecast.json",
            ],
            workingDirectory);

        Assert.True(options.UseRag);
        Assert.True(options.UseTools);
        Assert.Equal(
            Path.Combine(workingDirectory, "reference.csproj"),
            options.ServerProject);
        Assert.Equal(
            Path.Combine(workingDirectory, "forecast.json"),
            options.FixturePath);

        IReadOnlyDictionary<string, string?> childEnvironment =
            McpToolSession.CreateChildEnvironment(options.FixturePath);
        Assert.Equal(string.Empty, childEnvironment["FOUNDRY_MODEL_API_KEY"]);
        Assert.Equal(string.Empty, childEnvironment["FOUNDRY_EMBEDDING_DEPLOYMENT"]);
        Assert.Equal(string.Empty, childEnvironment["OPENWEATHER_API_KEY"]);
        Assert.Equal(options.FixturePath, childEnvironment["STORMWATCH_FIXTURE_PATH"]);
    }

    private static int CountOccurrences(string value, string search) =>
        (value.Length - value.Replace(search, string.Empty, StringComparison.Ordinal).Length)
        / search.Length;

    private sealed class RecordingSession : IChatSession
    {
        public List<string> Questions { get; } = [];

        public Task<string> AskAsync(
            string question,
            CancellationToken cancellationToken = default)
        {
            Questions.Add(question);
            return Task.FromResult($"Answer {Questions.Count}");
        }
    }

    private sealed class KeywordEmbeddingService : ITextEmbeddingService
    {
        public List<IReadOnlyList<string>> Requests { get; } = [];

        public Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateAsync(
            IReadOnlyList<string> inputs,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(inputs.ToArray());
            IReadOnlyList<ReadOnlyMemory<float>> vectors = inputs
                .Select(input => input.Contains("shelter", StringComparison.OrdinalIgnoreCase) ||
                    input.Contains("indoors", StringComparison.OrdinalIgnoreCase)
                        ? new ReadOnlyMemory<float>([1f, 0f, 0f])
                        : input.Contains("flood", StringComparison.OrdinalIgnoreCase) ||
                            input.Contains("higher ground", StringComparison.OrdinalIgnoreCase)
                            ? new ReadOnlyMemory<float>([0f, 1f, 0f])
                            : new ReadOnlyMemory<float>([0f, 0f, 1f]))
                .ToArray();
            return Task.FromResult(vectors);
        }
    }

    private sealed class ConstantEmbeddingService(ReadOnlyMemory<float> vector)
        : ITextEmbeddingService
    {
        public Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateAsync(
            IReadOnlyList<string> inputs,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ReadOnlyMemory<float>>>(
                inputs.Select(_ => vector).ToArray());
    }

    private sealed class StaticKnowledgeRetriever(IReadOnlyList<KnowledgePassage> passages)
        : IKnowledgeRetriever
    {
        public Task<IReadOnlyList<KnowledgePassage>> RetrieveAsync(
            string question,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(passages);
    }

    private sealed class RecordingCompletionService(params string[] answers)
        : IChatCompletionService
    {
        private readonly Queue<string> _answers = new(answers);

        public List<IReadOnlyList<ChatMessage>> Requests { get; } = [];

        public Task<string> CompleteAsync(
            IReadOnlyList<ChatMessage> messages,
            IReadOnlyList<AITool> tools,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(messages.ToArray());
            return Task.FromResult(_answers.Dequeue());
        }
    }
}