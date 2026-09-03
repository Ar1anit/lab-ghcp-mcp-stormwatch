using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

LoadDotEnv(Path.Combine(Environment.CurrentDirectory, ".env"));

if (args.Length is < 2 or > 3)
{
    Console.Error.WriteLine(
        "Usage: StormWatch.FoundryClient <server-project> <fixture-path> [city]");
    return 2;
}

string endpoint = RequireEnvironmentVariable("FOUNDRY_MODEL_ENDPOINT");
string deployment = RequireEnvironmentVariable("FOUNDRY_MODEL_DEPLOYMENT");
string apiKey = RequireEnvironmentVariable("FOUNDRY_MODEL_API_KEY");
string serverProject = Path.GetFullPath(args[0]);
string fixturePath = Path.GetFullPath(args[1]);
string city = args.Length == 3 ? args[2] : "Bengaluru";

if (!File.Exists(serverProject))
{
    throw new FileNotFoundException("The MCP server project was not found.", serverProject);
}

if (!File.Exists(fixturePath))
{
    throw new FileNotFoundException("The forecast fixture was not found.", fixturePath);
}

var transport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "StormWatch local MCP server",
    Command = "dotnet",
    Arguments = ["run", "--project", serverProject, "--", "--mcp"],
    EnvironmentVariables = new Dictionary<string, string?>
    {
        ["STORMWATCH_FIXTURE_PATH"] = fixturePath,
    },
});

await using McpClient mcpClient = await McpClient.CreateAsync(transport);
IList<McpClientTool> tools = await mcpClient.ListToolsAsync();
string[] expectedTools = ["assess_storm_risk", "get_forecast"];
string[] discoveredTools = tools.Select(tool => tool.Name).Order().ToArray();

if (!discoveredTools.SequenceEqual(expectedTools))
{
    throw new InvalidOperationException(
        $"Expected {string.Join(", ", expectedTools)}; found " +
        $"{string.Join(", ", discoveredTools)}.");
}

IChatClient chatClient = new ChatClientBuilder(
    new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey))
            .GetChatClient(deployment)
            .AsIChatClient())
    .UseFunctionInvocation()
    .Build();

var messages = new List<ChatMessage>
{
    new(
        ChatRole.System,
        "You are a weather-risk assistant. Use the available StormWatch tools " +
        "for weather claims. State that results are educational signals, not " +
        "official warnings."),
    new(
        ChatRole.User,
        $"Assess storm risk for {city}. Include the peak time, score, level, " +
        "and evidence."),
};

ChatResponse response = await chatClient.GetResponseAsync(
    messages,
    new ChatOptions { Tools = [.. tools] });

Console.WriteLine(response.Text);
return 0;

static string RequireEnvironmentVariable(string name)
{
    string? value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Set {name} before running the client.");
    }

    return value;
}

static void LoadDotEnv(string path)
{
    if (!File.Exists(path))
    {
        return;
    }

    int lineNumber = 0;
    foreach (string line in File.ReadLines(path))
    {
        lineNumber++;
        string trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#'))
        {
            continue;
        }

        int separator = trimmed.IndexOf('=');
        if (separator <= 0)
        {
            throw new InvalidOperationException(
                $"Invalid .env entry on line {lineNumber}.");
        }

        string name = trimmed[..separator].Trim();
        string value = trimmed[(separator + 1)..].Trim();
        if (value.Length >= 2 &&
            ((value[0] == '"' && value[^1] == '"') ||
             (value[0] == '\'' && value[^1] == '\'')))
        {
            value = value[1..^1];
        }

        if (Environment.GetEnvironmentVariable(name) is null)
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}
