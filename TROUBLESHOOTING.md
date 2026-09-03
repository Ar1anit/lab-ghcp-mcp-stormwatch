# StormWatch Environment Troubleshooting

Use this page only for environment, package restore, network, fixture, and MCP
startup failures. Implementation decisions are part of the challenge.

Never paste credentials or unfiltered logs into Copilot Chat.

## .NET Environment

### `dotnet` is not recognized

Install the .NET 10 SDK from <https://dotnet.microsoft.com/download/dotnet/10.0>,
restart VS Code, and verify that `dotnet --list-sdks` includes a 10.x SDK.

### Restore fails

Confirm that the venue network permits access to the NuGet sources configured
for the repository. On a managed network, use only the package source and
certificate configuration approved by your organization. Do not disable TLS
verification.

The project uses the committed `ModelContextProtocol` package version. A restore
failure is an environment issue, not an implementation task.

## Starter Baseline

The untouched starter must:

- compile successfully;
- discover 17 tests; and
- fail at the four intentional incomplete methods.

A missing fixture, test discovery failure, package restore error, or compiler
error before any participant edit is a setup problem. Ask the facilitator for a
clean starter copy.

## OpenWeather Access

### HTTP 401

- Allow time for a newly created key to activate.
- Confirm that the key can access Direct Geocoding and 5 Day / 3 Hour Forecast.
- Re-enter it through the masked VS Code input.
- Never hard-code or print the key.

### HTTP 429, proxy, certificate, or timeout

Stop repeated retries. Use the proxy, certificate, and package/network routes
approved by your organization. Do not disable TLS verification.

The supplied fixture is the deterministic offline acceptance path when live
OpenWeather access is unavailable.

## MCP Startup

### Server is not listed

- Confirm that `starter/` is the open VS Code workspace root.
- Confirm that `.vscode/mcp.json` exists.
- Confirm that local MCP servers are permitted by organizational policy.
- Open **MCP: List Servers** and inspect the `stormwatch` server status.

### Server fails before tool discovery

Verify that the StormWatch project builds. Then inspect the server output for
package, executable, configuration, or policy errors. Do not share output until
credentials and local identifiers have been removed.

### Tools still show an older schema

Restart the `stormwatch` server and start a fresh Copilot Chat so VS Code can
rediscover the current tool definitions.

## Credential Incident

If an API key appears in source, chat, a command, a log, or a screenshot, rotate
it in OpenWeather immediately. Removing the visible text is not sufficient after
exposure.
