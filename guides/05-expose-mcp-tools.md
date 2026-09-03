# Task 5: Expose MCP Tools

**Timebox:** 10 minutes

**Clock:** 0:44-0:54
**Primary file:** `stormwatch/StormWatchTools.cs`

## Outcome

Expose the completed application through two concise, structured MCP tools using
the official `ModelContextProtocol` C# SDK. The tools must compose existing
services, preserve cancellation, avoid credentials, and keep stdout reserved for
the stdio protocol.

## Step 1: Understand the Existing Host

Open:

- `stormwatch/Program.cs`;
- `stormwatch/StormWatchTools.cs`;
- `stormwatch/StormWatch.csproj`; and
- `.vscode/mcp.json`.

In `Program.cs`, identify:

```csharp
AddMcpServer()
WithStdioServerTransport()
WithToolsFromAssembly()
```

Also confirm that logging providers are cleared in MCP mode. With stdio
transport, stdout carries JSON-RPC protocol messages; ordinary console output
can corrupt the protocol.

In `StormWatchTools.cs`, identify:

- `[McpServerToolType]` on the tool class;
- `[McpServerTool]` and descriptions on each method;
- the injected `StormWatchDataService`;
- `ForecastToolResult` and `AssessmentToolResult`; and
- the existing domain services the methods should compose.

## Step 2: Write a Thin-Adapter Prompt

Write your own Copilot prompt for the two TODO methods in
`StormWatchTools.cs`. State the file and method boundary, official SDK and
structured-result constraints, existing services to compose, credential and
stdio rules, and the build evidence you expect. The prompt should describe the
contract without supplying method bodies.

## Step 3: Review `get_forecast`

The method should:

1. await `dataService.LoadAsync(city, cancellationToken)`;
2. return the resolved display name and coordinates;
3. state whether the source is OpenWeather or the synthetic fixture;
4. declare units explicitly;
5. map at most the next five points; and
6. preserve typed numeric and timestamp fields.

Do not return a preformatted CLI string. Structured fields let the MCP client
reason over the result without parsing presentation text.

## Step 4: Review `assess_storm_risk`

The method should:

1. load the forecast through the same data service;
2. call `StormRiskService.Assess`;
3. return level, score, peak UTC time, and evidence;
4. retain source and resolved location; and
5. include the educational, non-official disclaimer.

There must be no copy of the scoring thresholds in the MCP class.

## Step 5: Build Before Starting the Server

Run:

```powershell
dotnet build .\stormwatch\StormWatch.csproj
```

Resolve compiler or analyzer errors before asking VS Code to start the server.

Inspect the diff:

```powershell
git diff -- .\stormwatch\StormWatchTools.cs
git diff --name-only
```

Only the two TODO method bodies should require implementation.

## Step 6: Choose Live or Deterministic Fixture Mode

For live mode, keep the masked input in `.vscode/mcp.json`:

```json
"env": {
  "OPENWEATHER_API_KEY": "${input:openweather-api-key}"
}
```

Enter the key only in the masked VS Code prompt.

If the network or key is unreliable, use the approved fixture fallback:

```json
"env": {
  "STORMWATCH_FIXTURE_PATH": "${workspaceFolder}/tests/fixtures/forecast.json"
}
```

Restart the server after changing configuration. Do not place the key directly
in JSON.

## Step 7: Start and Inspect the Server

In VS Code:

1. Run **MCP: List Servers**.
2. Select `stormwatch`.
3. Choose **Start** or **Restart**.
4. Open Copilot Agent mode.
5. Open **Tools** and find:
   - `get_forecast`
   - `assess_storm_risk`
6. Inspect each tool description and input schema before invoking it.

If methods were already discoverable while throwing `NotImplementedException`,
restart the server so VS Code loads the completed assembly.

## If You Are Stuck

- If the server is missing, confirm `starter/` is the workspace root.
- If it exits, run the build and inspect **Show Output**.
- If JSON-RPC parsing fails, remove all stdout application logging.
- If tools are stale, restart the server and Chat.
- If live requests fail, switch to the synthetic fixture.

## Completion Gate

You can say **“Task 5 is done”** only when all of the following are true:

- [ ] `dotnet build .\stormwatch\StormWatch.csproj` succeeds.
- [ ] VS Code discovers exactly the intended `get_forecast` and
      `assess_storm_risk` tools from the official SDK attributes.
- [ ] Both methods await the injected data service, pass cancellation through,
      and contain no HTTP, JSON, environment, or threshold duplication.
- [ ] Forecast output is structured and includes resolved location,
      coordinates, source, units, and no more than five typed periods.
- [ ] Assessment output is structured and includes level, score, peak UTC time,
      evidence, source, and the educational disclaimer.
- [ ] No API key or credential-bearing URL can appear in either tool result.
- [ ] MCP mode writes no ordinary application output to stdout, and logging
      providers remain cleared for stdio.
- [ ] You can explain which responsibilities the SDK provides—transport,
      discovery, schema, and invocation—and which security decisions remain with
      the developer and user.

**Evidence to retain:** a successful build and the VS Code Tools view showing
both names with their structured schemas.

Continue to [Task 6: Validate and review](06-validate-and-review.md).
