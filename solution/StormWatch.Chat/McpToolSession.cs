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

    public static async Task<McpToolSession> ConnectAsync(
        string serverProject,
        string weatherEnvironmentFile,
        string? fixturePath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(serverProject))
        {
            throw new FileNotFoundException("The MCP server project was not found.", serverProject);
        }

        var arguments = new List<string> { "run", "--project", serverProject, "--", "--mcp" };
        IReadOnlyDictionary<string, string?> environment = CreateChildEnvironment(fixturePath);
        if (fixturePath is not null)
        {
            if (!File.Exists(fixturePath))
            {
                throw new FileNotFoundException("The forecast fixture was not found.", fixturePath);
            }
        }
        else
        {
            arguments.Add("--env-file");
            arguments.Add(weatherEnvironmentFile);
        }

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "StormWatch local MCP server",
            Command = "dotnet",
            Arguments = arguments,
            EnvironmentVariables = new Dictionary<string, string?>(environment),
        });
        McpClient client = await McpClient.CreateAsync(transport, cancellationToken: cancellationToken);
        try
        {
            IList<McpClientTool> discovered = await client.ListToolsAsync(cancellationToken: cancellationToken);
            string[] expected = ["assess_storm_risk", "get_forecast"];
            string[] actual = discovered.Select(tool => tool.Name).Order().ToArray();
            if (!actual.SequenceEqual(expected))
            {
                throw new InvalidOperationException(
                    $"Expected {string.Join(", ", expected)}; found {string.Join(", ", actual)}.");
            }

            return new McpToolSession(client, [.. discovered]);
        }
        catch
        {
            await client.DisposeAsync();
            throw;
        }
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