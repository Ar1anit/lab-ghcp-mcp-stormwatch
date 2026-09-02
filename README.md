# StormWatch: GitHub Copilot x MCP C# Developer Lab

Build a .NET 10 application that reads OpenWeather's five-day forecast,
calculates a transparent storm-risk signal, and exposes it to GitHub Copilot
through a local server built with the official Model Context Protocol C# SDK.

- **Duration:** 60 minutes
- **Audience:** Developers with basic C# and Git familiarity
- **Format:** Individual or pairs, guided checkpoints
- **Result:** A tested CLI plus two structured MCP tools in Copilot Agent mode

> StormWatch is an educational risk indicator, not a meteorological forecast,
> emergency alert, medical device, or safety system. Use official local weather
> services for operational decisions.

## Learning Outcomes

You will:

- give Copilot focused repository context and executable acceptance criteria;
- turn OpenWeather JSON into immutable C# records through an `HttpClient` adapter;
- use xUnit tests to steer and verify generated code without live network calls;
- keep credentials out of source, chat, logs, and tool results; and
- expose existing application services with the official MCP C# SDK over stdio.

## Participant Guides

Use the six step-by-step guides during the timed lab. Each guide ends with a
completion gate that checks behavior, design, evidence, security, and
understanding—not just whether a file was edited.

1. [Orient and verify](guides/01-orient-and-verify.md) — 7 minutes
2. [Build the weather adapter](guides/02-build-weather-adapter.md) — 13 minutes
3. [Implement storm-risk rules](guides/03-implement-risk-rules.md) — 15 minutes
4. [Complete the CLI](guides/04-complete-cli.md) — 9 minutes
5. [Expose MCP tools](guides/05-expose-mcp-tools.md) — 10 minutes
6. [Validate and review](guides/06-validate-and-review.md) — 6 minutes

See [the participant guide index](guides/README.md) for the complete route,
working conventions, and recovery rules.

## Architecture

```text
City -> OpenWeather geocoding -> coordinate forecast -> Forecast records
                                                        |          |
                                                        v          v
                                               risk service     CLI report
                                                        |
                                                        v
                                  official MCP C# SDK tools over stdio
                                                        |
                                                        v
                                             GitHub Copilot in VS Code
```

The MCP host is deliberately thin. `Program.cs` registers the official SDK with
`AddMcpServer()`, `WithStdioServerTransport()`, and `WithToolsFromAssembly()`.
`StormWatchTools.cs` marks the tool class and methods with
`[McpServerToolType]` and `[McpServerTool]`.

## Before The Timed Lab

1. Install the .NET 10 SDK, Git, VS Code, and the C# Dev Kit extension.
2. Install GitHub Copilot and GitHub Copilot Chat, then confirm Agent mode works.
3. Create an OpenWeather API key at <https://home.openweathermap.org/api_keys>.
4. Confirm access to Direct Geocoding and the 5 Day / 3 Hour Forecast APIs.
5. Open `starter/` as the VS Code workspace root and run:

```powershell
dotnet --version
dotnet restore .\StormWatch.Tests\StormWatch.Tests.csproj
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```

The 17 tests should build and fail at four intentional `TODO` checkpoints.
Never paste an API key into Copilot Chat, source, screenshots, or commands. VS
Code requests it through a masked MCP input.

## The 60-Minute Route

| Time | Checkpoint | Outcome |
| --- | --- | --- |
| 0:00-0:07 | [Orient and verify](guides/01-orient-and-verify.md) | Understand the red baseline and boundaries. |
| 0:07-0:20 | [Build the weather adapter](guides/02-build-weather-adapter.md) | OpenWeather JSON becomes typed records. |
| 0:20-0:35 | [Implement risk rules](guides/03-implement-risk-rules.md) | Scoring is deterministic and explainable. |
| 0:35-0:44 | [Complete the CLI](guides/04-complete-cli.md) | City or fixture produces a safe report. |
| 0:44-0:54 | [Implement MCP tools](guides/05-expose-mcp-tools.md) | Official SDK exposes two structured tools. |
| 0:54-1:00 | [Validate and review](guides/06-validate-and-review.md) | Tests pass and one tool call is inspected. |

## 0:00-0:07 - Orient And Verify

Inspect `StormWatch/Models.cs`, the test project, and the four TODO files:

- `StormWatch/Weather.cs`: HTTP requests and JSON parsing;
- `StormWatch/Risk.cs`: deterministic scoring;
- `StormWatch/StormWatchApp.cs`: CLI orchestration and rendering;
- `StormWatch/StormWatchTools.cs`: MCP SDK wrappers.

Ask Copilot in Agent mode:

```text
Read README.md, StormWatch/Models.cs, and StormWatch.Tests. Summarize the
architecture, four TODO checkpoints, and security constraints. Do not edit.
Explain which code is application logic and which code is MCP transport.
```

**Checkpoint:** You can explain why tests never call the live API and why risk
logic does not belong in the MCP methods.

## 0:07-0:20 - Build The Weather Adapter

Open `Weather.cs`, `WeatherTests.cs`, and `tests/fixtures/forecast.json`.

```text
Implement only StormWatch/Weather.cs so WeatherTests passes. Use the injected
HttpClient. Resolve the city through OpenWeather Direct Geocoding, then request
the forecast by latitude and longitude with metric units. Use System.Text.Json,
a finite cancellation timeout, useful errors, and invariant-culture numbers.
Never include the API key or credential-bearing URL in an exception. Do not
change tests or public records. Run only WeatherTests.
```

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj --filter FullyQualifiedName~WeatherTests
```

Review that missing `rain` becomes `0.0`, requests use HTTPS, city values are
encoded, and status errors contain no key.

## 0:20-0:35 - Implement Storm-Risk Rules

Each three-hour point earns:

| Indicator | Points |
| --- | ---: |
| Weather code `200-232` | 60 |
| Wind at least `15 m/s` | 25 |
| Otherwise wind at least `10 m/s` | 15 |
| Rain at least `10 mm/3h` | 20 |
| Otherwise rain at least `5 mm/3h` | 10 |
| Pressure at most `990 hPa` | 15 |
| Otherwise pressure at most `1000 hPa` | 8 |

Cap the score at 100. Levels are `low` for 0-29, `watch` for 30-59,
and `warning` for 60-100. Select the highest score; ties use the earliest time.

```text
Implement only StormWatch/Risk.cs from README.md and RiskTests.cs. Keep tiered
thresholds mutually exclusive, return a reason for every awarded indicator,
reject an empty forecast, and make tie-breaking deterministic. Run RiskTests.
```

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj --filter FullyQualifiedName~RiskTests
```

## 0:35-0:44 - Complete The CLI

```text
Implement StormWatch/StormWatchApp.cs. Accept <city> [--fixture <path>]. Fixture
mode must not require OPENWEATHER_API_KEY. Print five points, peak level, score,
time, evidence, and the disclaimer. Handle expected errors with exit code 2 and
no stack trace or secret. Run AppTests, then all tests.
```

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj --filter FullyQualifiedName~AppTests
dotnet run --project .\StormWatch -- Bengaluru --fixture .\tests\fixtures\forecast.json
```

**Checkpoint:** The fixture reports `WARNING (100/100)` and all 17 tests pass.

## 0:44-0:54 - Expose Official SDK Tools

Read `Program.cs`, `StormWatchTools.cs`, and `.vscode/mcp.json`.

```text
Complete only the two TODO methods in StormWatch/StormWatchTools.cs. Keep the
official ModelContextProtocol SDK attributes and structured record return types.
Compose StormWatchDataService and StormRiskService; do not duplicate HTTP,
parsing, or scoring logic. Preserve fixture fallback and cancellation. Do not
return credentials. Build the project after editing.
```

```powershell
dotnet build .\StormWatch\StormWatch.csproj
```

In VS Code, run **MCP: List Servers**, select `stormwatch`, and choose **Start**.
Enter the key in the masked prompt. In Copilot Agent mode, open **Tools** and
confirm `get_forecast` and `assess_storm_risk` are available.

The MCP process runs with your user permissions. Review `.vscode/mcp.json` and
each proposed tool call before trusting or approving it.

## 0:54-1:00 - Validate And Review

Ask Copilot:

```text
Use stormwatch assess_storm_risk for Bengaluru. State the peak time, score,
level, and evidence. Label it as an educational signal, not an official warning.
```

Then run:

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```

```text
Review the completed project against README.md. List only concrete correctness,
security, MCP contract, or test coverage gaps. Cite files. If there are no
findings, say so and name the largest remaining limitation.
```

## Stretch Goals

1. Add metric/imperial display while keeping scoring internally metric.
2. Add a structured tool that compares two cities deterministically.
3. Return the top three risk windows.
4. Cache successful forecasts for five minutes without caching failures.
5. Add MCP SDK client integration tests to the participant project.

## Responsible Use

- OpenWeather can be delayed, incomplete, or unavailable.
- Thresholds do not model storm formation or local impact.
- Never use this output for emergency, clinical, infrastructure, travel, or
  public-safety decisions.
- Give every MCP server only the credentials and file access it needs.

## Official References

- [Develop with the MCP C# SDK](https://learn.microsoft.com/dotnet/ai/get-started-mcp)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [Add and manage MCP servers in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers)
- [OpenWeather Geocoding API](https://openweathermap.org/api/geocoding-api)
- [OpenWeather 5 Day / 3 Hour Forecast](https://openweathermap.org/forecast5)

The lab was converted and verified on 2026-09-02. Recheck prerelease MCP SDK
versions and VS Code configuration before delivering after a dependency update.