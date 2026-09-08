using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace StormWatch.Chat;

public sealed class McpToolSession : IAsyncDisposable
{
    private static readonly string[] ParentOnlyVariables =
    [
        "FOUNDRY_MODEL_ENDPOINT",
        "FOUNDRY_MODEL_DEPLOYMENT",
        "FOUNDRY_EMBEDDING_DEPLOYMENT",
        "FOUNDRY_MODEL_API_KEY",
        "OPENWEATHER_API_KEY",
    ];

    private readonly McpClient _client;

    private McpToolSession(McpClient client, IReadOnlyList<AITool> tools)
    {
        _client = client;
        Tools = tools;
    }

    public IReadOnlyList<AITool> Tools { get; }

    public static Task<McpToolSession> ConnectAsync(
        string serverProject,
        string weatherEnvironmentFile,
        string? fixturePath,
        CancellationToken cancellationToken = default)
    {
        _ = serverProject;
        _ = weatherEnvironmentFile;
        _ = fixturePath;
        // TODO 6: Start the local MCP server and return exactly its two tools.
        throw new NotImplementedException();
    }

    public static IReadOnlyDictionary<string, string?> CreateChildEnvironment(
        string? fixturePath)
    {
        var environment = ParentOnlyVariables.ToDictionary(
            name => name,
            _ => (string?)string.Empty,
            StringComparer.Ordinal);
        if (fixturePath is not null)
        {
            environment["STORMWATCH_FIXTURE_PATH"] = fixturePath;
        }

        return environment;
    }

    public ValueTask DisposeAsync() => _client.DisposeAsync();
}