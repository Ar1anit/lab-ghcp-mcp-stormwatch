# Task 6: Connect OpenWeather MCP Tools

**Timebox:** 14 minutes  
**Clock:** 1:11-1:25  
**Primary files:** `stormwatch/StormWatchTools.cs`, `StormWatch.Chat/McpToolSession.cs`

## Outcome

Expose forecast and assessment capabilities through the official MCP C# SDK,
then give those same local tools to both the StormWatch chatbot and GitHub
Copilot Agent mode.

## Step 1: Complete Thin MCP Tools

Open `StormWatchTools.cs`, `Models.cs`, and the completed weather/risk services.
Implement exactly:

- `get_forecast`, returning location, coordinates, source, units, and five
  typed periods; and
- `assess_storm_risk`, returning level, score, peak time, evidence, source, and
  the educational disclaimer.

Both methods must call `StormWatchDataService`. Do not duplicate HTTP requests,
JSON parsing, thresholds, or answer generation. Preserve cancellation, and keep
stdout reserved for MCP protocol messages.

## Step 2: Connect the Chatbot

In `McpToolSession.ConnectAsync`:

1. validate the server project and optional fixture path;
2. start `dotnet run --project <server> -- --mcp` over stdio;
3. use `STORMWATCH_FIXTURE_PATH` for deterministic mode;
4. otherwise pass `--env-file <path>` so the server loads only
   `.env.openweather`;
5. call `CreateChildEnvironment` so inherited Foundry model, embedding, and
  OpenWeather values are blanked before the child starts;
6. discover tools through `McpClient`; and
7. reject any set other than exactly `get_forecast` and
   `assess_storm_risk`.

The Foundry `.env` must never be passed to the MCP child process.

## Step 3: Build and Run the Chatbot with Tools

```powershell
dotnet build .\stormwatch\StormWatch.csproj
dotnet build .\StormWatch.Chat\StormWatch.Chat.csproj
dotnet run --project .\StormWatch.Chat -- --with-tools `
  --fixture .\tests\fixtures\forecast.json
```

Ask for Bengaluru's storm risk. The answer should use the MCP assessment result,
include `WARNING (100/100)`, and preserve the educational qualification. Then
ask a preparedness question; it should use local `rag-data` sources rather than
treating weather tool output as general safety guidance.

## Step 4: Validate in GitHub Copilot

For live mode, `.vscode/mcp.json` loads only `.env.openweather`. For deterministic
mode, temporarily replace its server environment setting with:

```json
"env": {
  "STORMWATCH_FIXTURE_PATH": "${workspaceFolder}/tests/fixtures/forecast.json"
}
```

Restart `stormwatch` from **MCP: List Servers**, open Agent mode, inspect Tools,
and deliberately invoke one tool. Review the city argument before approval.

## Completion Gate

- [ ] The server advertises exactly `get_forecast` and `assess_storm_risk`.
- [ ] Both tools return concise structured values from shared services.
- [ ] The chatbot invokes a fixture-backed MCP tool and reports the expected
      Bengaluru result.
- [ ] GitHub Copilot invokes one of the same local tools after deliberate
      approval.
- [ ] Local-file evidence and current weather data retain distinct source labels.
- [ ] The MCP process receives only fixture configuration or
      `.env.openweather`, never chat or embedding model credentials.
- [ ] `ConnectAsync` uses `CreateChildEnvironment`; it does not rely on the
  parent process environment being clean.
- [ ] MCP stdout contains protocol traffic only.

**Evidence to retain:** discovered tool names, one chatbot tool-backed answer,
and one inspected Copilot invocation.

Continue to [Task 7: Test the complete chatbot](07-test-complete-chatbot.md).
