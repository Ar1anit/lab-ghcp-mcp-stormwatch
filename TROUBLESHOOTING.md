# StormWatch Environment Troubleshooting

Use this page for environment, restore, chat model, embedding, local RAG,
OpenWeather, and MCP
startup failures. Implementation decisions remain part of the exercise.

Never paste credentials, environment-file contents, full request URLs, or
unfiltered logs into Copilot Chat.

## .NET and Restore

### `dotnet` is not recognized

Install the .NET 10 SDK, restart VS Code, and confirm `dotnet --list-sdks`
contains a `10.x` entry. A runtime without the SDK is insufficient.

### Package restore fails

Confirm the venue network permits the configured NuGet sources. Use only your
organization's approved proxy, package source, and certificate configuration;
never disable TLS validation.

## Starter Baseline

The untouched starter must compile and discover 22 tests across:

```powershell
dotnet test .\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```

Expected failures are `NotImplementedException` paths in the numbered TODOs.
Compiler errors, missing fixtures, package failures, or undiscovered tests are
setup problems. Ask the facilitator for a clean starter copy.

## Foundry Chat

### Authentication or deployment failure

Confirm `.env` is directly inside `starter/` and contains the three
chat settings plus `FOUNDRY_EMBEDDING_DEPLOYMENT`. Confirm both deployment names
belong to the endpoint. Ask the facilitator to replace an expired file; do not
reveal its values.

### Rate limit or timeout

Stop repeated retries. The facilitator should inspect quota and use the planned
staggered test window. Repeated model calls increase load and cost.

### Chat exits or returns no answer

Run `dotnet build .\StormWatch.Chat\StormWatch.Chat.csproj`. In Task 2, use
`--no-rag` so missing corpus or embedding configuration cannot block the
model-only chatbot.
An empty model response is an error and should not be stored as conversation
history.

## Local RAG and Embeddings

### `rag-data` is missing or empty

Confirm the facilitator-provided `rag-data` folder is directly inside
`starter/` and contains non-empty `.md` or `.txt` files. Other extensions are
ignored. Do not add credentials, patient data, or other restricted content.

### Embedding authentication or deployment failure

Ask the facilitator to confirm that `FOUNDRY_EMBEDDING_DEPLOYMENT` names an
embedding deployment on the endpoint in `.env` and that the temporary key can
invoke it. Do not print the file, deployment response, or credential-bearing
request details.

### Corpus limit error

Keep the supplied corpus within 100 supported files, 512,000 bytes per file,
and 500 total chunks. Split or remove oversized educational files rather than
raising limits during the timed lab.

### Retrieval returns no passages or weak matches

Try the facilitator-provided known-good corpus question and verify the expected
terms appear in the local files. A genuinely unsupported question should fall
below the similarity threshold and produce bounded uncertainty, not a fabricated
answer or sources from an earlier turn.

### Embedding deployment is unavailable

Run model-only chat with `--no-rag`. To continue the MCP portion as well, use:

```powershell
dotnet run --project .\StormWatch.Chat -- --no-rag --with-tools `
  --fixture .\tests\fixtures\forecast.json
```

Do not claim that model-only answers came from `rag-data`.

## OpenWeather

### HTTP 401

Ask the facilitator to verify or replace `.env.openweather`, allow time for a
new key to activate, then restart the MCP server. Never hard-code or print it.

### HTTP 429, proxy, certificate, or timeout

Stop repeated retries. Use the approved proxy and certificate path. Switch to
the synthetic fixture for deterministic acceptance.

## Offline MCP Fallback

For the chatbot, run:

```powershell
dotnet run --project .\StormWatch.Chat -- --with-tools `
  --fixture .\tests\fixtures\forecast.json
```

For GitHub Copilot, temporarily replace the `envFile` property in
`.vscode/mcp.json` with:

```json
"env": {
  "STORMWATCH_FIXTURE_PATH": "${workspaceFolder}/tests/fixtures/forecast.json"
}
```

Restart `stormwatch` from **MCP: List Servers**. Restore
`"envFile": "${workspaceFolder}/.env.openweather"` before demonstrating live
data.

## MCP Startup

### Server is not listed

- Confirm `starter/` is the open workspace root.
- Confirm `.vscode/mcp.json` exists and local MCP servers are allowed by policy.
- Run **MCP: List Servers** and inspect `stormwatch`.

### Server fails before discovery

Build both projects. Confirm fixture and environment-file paths exist without
opening those files. Sanitize any output before sharing it.

### Tools are missing or stale

The server must advertise exactly `get_forecast` and `assess_storm_risk`.
Complete both TODO methods, rebuild, restart the server, and begin a fresh Chat
session so VS Code rediscovers schemas.

## Credential Incident

If any API key appears in source, chat, a command, log, screenshot, tool output,
or Git history, notify the facilitator and rotate or revoke it immediately.
Deleting visible text is not sufficient after exposure.
