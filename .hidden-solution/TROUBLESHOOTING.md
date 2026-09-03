# StormWatch Troubleshooting

Start with the smallest failing boundary: .NET, tests, OpenWeather, MCP server,
or Copilot invocation. Never paste credentials or unfiltered logs into chat.

## .NET Environment

### `dotnet` is not recognized

Install the .NET 10 SDK from <https://dotnet.microsoft.com/download/dotnet/10.0>,
then restart VS Code and verify:

```powershell
dotnet --info
```

An older runtime alone is insufficient; `dotnet --list-sdks` must include 10.x.

### Restore or package errors

```powershell
dotnet nuget locals all --clear
dotnet restore .\StormWatch.Tests\StormWatch.Tests.csproj
```

The lab targets `ModelContextProtocol` `0.4.0-preview.3`. Prerelease SDK APIs can
change; use the committed package version during the workshop.

## Expected Red Tests

The untouched starter compiles and fails with `NotImplementedException` at four
TODO checkpoints. Collection failures, missing fixtures, restore errors, and C#
compiler errors are setup defects.

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj --filter FullyQualifiedName~WeatherTests
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj --filter FullyQualifiedName~RiskTests
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj --filter FullyQualifiedName~AppTests
```

## OpenWeather

### HTTP 401

- Wait for a newly created key to activate.
- Confirm Geocoding and 5 Day / 3 Hour Forecast access.
- Re-enter the masked MCP input; never hard-code the key.

### HTTP 404 or location not found

Use a less ambiguous city such as `Bengaluru,IN`. The adapter must geocode first
and call the forecast API with coordinates.

### HTTP 429, proxy, certificate, or timeout

Stop repeated retries. Use the approved organizational proxy/certificate setup;
do not disable TLS. Switch to fixture mode for a deterministic offline path.

## Offline MCP Fallback

In `.vscode/mcp.json`, replace the server `env` object with:

```json
"env": {
  "STORMWATCH_FIXTURE_PATH": "${workspaceFolder}/tests/fixtures/forecast.json"
}
```

Restart `stormwatch` from **MCP: List Servers**. Both tools now use the synthetic
Bengaluru data without requesting a key. Restore the masked key configuration
before demonstrating live data.

## MCP Server

### Server is not listed

- Open `starter/` as the workspace root.
- Run **MCP: Open Workspace Folder Configuration** and inspect `.vscode/mcp.json`.
- Run **MCP: List Servers**, select `stormwatch`, then start or restart it.
- Confirm local MCP servers are allowed by organizational policy.

### Server exits immediately

```powershell
dotnet build .\StormWatch\StormWatch.csproj
dotnet run --project .\StormWatch -- --mcp
```

The second command waits silently for protocol input. Stop it with `Ctrl+C`.
Use **MCP: List Servers > stormwatch > Show Output** for SDK startup failures.

### JSON-RPC or protocol parsing errors

Do not call `Console.WriteLine` in MCP mode. Stdout carries protocol messages.
Diagnostics must go to stderr, and logging providers should remain cleared.

### Tools do not update

Restart the server so VS Code rediscovers schemas. If needed, clear cached tools
through the server action and restart Chat.

### Tools throw `NotImplementedException`

The server can discover attributed methods before their TODO bodies are done.
Complete both methods in `StormWatchTools.cs`, rebuild, and restart the server.

## Copilot Invocation

- Use Agent mode and enable only the needed StormWatch tool.
- Name the tool explicitly if another weather server is installed.
- Read the city argument before approval.
- A live `low` result is valid; use the fixture for a deterministic warning.

## Credential Cleanup

If a key reaches source, a command, chat, or screenshot, rotate it in
OpenWeather. Deleting visible text is not sufficient after exposure. The app
does not load `.env`; the masked VS Code MCP input is the supported live path.