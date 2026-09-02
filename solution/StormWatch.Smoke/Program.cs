using ModelContextProtocol.Client;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: StormWatch.Smoke <server-project> <fixture-path>");
    return 2;
}

string serverProject = Path.GetFullPath(args[0]);
string fixturePath = Path.GetFullPath(args[1]);
Environment.SetEnvironmentVariable("STORMWATCH_FIXTURE_PATH", fixturePath);
string dotnetDirectory = @"C:\Program Files\dotnet";
string currentPath = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
Environment.SetEnvironmentVariable("PATH", $"{dotnetDirectory};{currentPath}");

var transport = new StdioClientTransport(new StdioClientTransportOptions
{
    Name = "StormWatch reference",
    Command = "dotnet",
    Arguments = ["run", "--project", serverProject, "--", "--mcp"],
});

await using McpClient client = await McpClient.CreateAsync(transport);
IList<McpClientTool> tools = await client.ListToolsAsync();
string[] toolNames = tools.Select(tool => tool.Name).Order().ToArray();
string[] expected = ["assess_storm_risk", "get_forecast"];
if (!toolNames.SequenceEqual(expected))
{
    throw new InvalidOperationException(
        $"Expected {string.Join(", ", expected)}; found {string.Join(", ", toolNames)}.");
}

await client.CallToolAsync(
    "get_forecast",
    new Dictionary<string, object?> { ["city"] = "Bengaluru" });
await client.CallToolAsync(
    "assess_storm_risk",
    new Dictionary<string, object?> { ["city"] = "Bengaluru" });

Console.WriteLine("Discovered and invoked: assess_storm_risk, get_forecast");
return 0;