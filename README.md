# StormWatch: GitHub Copilot x MCP C# Developer Lab

Build a .NET 10 application that reads OpenWeather's five-day forecast,
calculates a transparent storm-risk signal, exposes it through a local server
using the official Model Context Protocol C# SDK, and optionally presents the
same result in a web frontend.

- **Core duration:** 60 minutes
- **Advanced task:** 20 minutes
- **Audience:** Developers with basic C# and Git familiarity
- **Starting point:** Open `starter/` as the VS Code workspace root
- **Result:** A tested CLI, two structured MCP tools, and an optional Razor Pages
  frontend

> StormWatch is an educational risk indicator, not a meteorological forecast,
> emergency alert, medical device, or safety system. Use official local weather
> services for operational decisions.

## Rules

- Use GitHub Copilot as a development partner, but decide what context and
  instructions it needs.
- Treat the existing tests and the outcomes below as the requirements.
- Do not change tests merely to make an implementation pass.
- Never place an OpenWeather API key in source, chat, commands, logs, or output.
- Unit tests must not call live services.
- Keep weather access, risk calculation, CLI presentation, and MCP transport as
  separate responsibilities.
- Use the official `ModelContextProtocol` C# SDK for MCP behavior.
- Keep all user-facing risk output labeled as an educational heuristic.

## Starting Check

From `starter/`, confirm that the .NET 10 SDK is available and the test project
restores. The untouched starter must compile, discover 17 tests, and fail only
at the four intentional implementation checkpoints.

## Core Challenge

| Time | Step | Goal |
| --- | --- | --- |
| 0:00-0:07 | 1. Understand the system | Identify the architecture, trust boundaries, and four incomplete responsibilities. |
| 0:07-0:20 | 2. Build the weather adapter | Convert OpenWeather responses into the supplied immutable domain records. |
| 0:20-0:35 | 3. Implement storm-risk rules | Produce deterministic, explainable risk assessments. |
| 0:35-0:44 | 4. Complete the CLI | Produce a safe report from either live city data or the offline fixture. |
| 0:44-0:54 | 5. Expose MCP tools | Make forecast and assessment capabilities available through the official SDK. |
| 0:54-1:00 | 6. Validate and review | Demonstrate correct behavior, secure handling, and one successful MCP invocation. |

## Step 1: Understand The System

### Goal

Build a correct mental model of the application before changing it.

### Required outcome

You can identify:

- the immutable domain model;
- the weather transport and parsing boundary;
- the deterministic risk boundary;
- the CLI presentation boundary;
- the MCP transport boundary;
- why tests use mocks and fixtures instead of live requests; and
- why application behavior must not be duplicated inside MCP tools.

### Completion evidence

- The starter builds and all 17 tests are discovered.
- Every failure maps to one of the four intentional incomplete responsibilities.
- You can explain where credentials enter the process and where they must never
  appear.

## Step 2: Build The Weather Adapter

### Goal

Implement OpenWeather access and parsing in `StormWatch/Weather.cs`.

### Required outcome

- A city is resolved through OpenWeather Direct Geocoding before forecast data
  is requested by latitude and longitude.
- Requests use HTTPS, metric units, encoded city values, cancellation, and a
  finite timeout.
- JSON is mapped into the supplied `Location`, `Forecast`, and `ForecastPoint`
  records.
- A missing rain value becomes `0.0`.
- Unknown cities, unsuccessful responses, timeouts, and invalid JSON produce
  useful domain-level errors.
- Errors contain neither the API key nor a credential-bearing URL.

### Completion evidence

All six `WeatherTests` pass without changing the tests or making a live network
request.

## Step 3: Implement Storm-Risk Rules

### Goal

Implement a pure and deterministic assessment in `StormWatch/Risk.cs`.

### Scoring requirements

| Indicator | Points |
| --- | ---: |
| Weather code `200-232` | 60 |
| Wind at least `15 m/s` | 25 |
| Otherwise wind at least `10 m/s` | 15 |
| Rain at least `10 mm/3h` | 20 |
| Otherwise rain at least `5 mm/3h` | 10 |
| Pressure at most `990 hPa` | 15 |
| Otherwise pressure at most `1000 hPa` | 8 |

### Required outcome

- Tiered thresholds are mutually exclusive.
- Every awarded indicator has a human-readable reason.
- The score is capped at 100.
- Scores `0-29` are `low`, `30-59` are `watch`, and `60-100` are `warning`.
- The highest-scoring forecast point is selected.
- Equal scores select the earliest timestamp.
- An empty forecast is rejected.

### Completion evidence

All eight `RiskTests` pass, including threshold boundaries and tie-breaking.

## Step 4: Complete The CLI

### Goal

Complete CLI orchestration and presentation in `StormWatch/StormWatchApp.cs`.

### Required outcome

- The application accepts a city and an optional fixture path.
- Fixture mode works without `OPENWEATHER_API_KEY`.
- Live mode obtains the key only from `OPENWEATHER_API_KEY`.
- The report contains the resolved location, five forecast points, peak level,
  score, time, evidence, source, and educational disclaimer.
- Numeric and timestamp output is deterministic across machine cultures.
- Expected user, weather, fixture, and configuration errors return exit code 2
  without a stack trace or secret.

### Completion evidence

- All three `AppTests` pass.
- All 17 tests pass together.
- The supplied fixture reports `WARNING (100/100)` with a peak at
  `2026-08-30 06:00 UTC`.

## Step 5: Expose MCP Tools

### Goal

Complete the two tools in `StormWatch/StormWatchTools.cs` using the official
C# MCP SDK.

### Required outcome

- The server advertises exactly `get_forecast` and `assess_storm_risk`.
- Both tools return the supplied structured result records.
- Tools compose the existing data and risk services rather than duplicating
  HTTP, parsing, scoring, or presentation logic.
- Fixture fallback and cancellation are preserved.
- Tool output is concise, deterministic, and free of credentials.
- The stdio server emits no application output that could corrupt protocol
  messages.

### Completion evidence

- The application builds.
- VS Code discovers both tools from the local MCP server.
- Each tool can be invoked successfully using fixture mode or an approved live
  OpenWeather connection.

## Step 6: Validate And Review

### Goal

Produce evidence that the implementation is correct, secure, and usable through
MCP.

### Required outcome

- All 17 tests pass without modification.
- The fixture CLI result matches the required score and peak time.
- One `assess_storm_risk` invocation returns structured forecast evidence.
- No credential appears in source, configuration output, exceptions, logs, or
  tool results.
- A review identifies any remaining correctness, security, MCP contract, or
  coverage gap.
- The largest limitation of the educational heuristic is stated explicitly.

### Completion evidence

Show the passing test summary and one inspected MCP result to the facilitator.

## Advanced Step 7: Build A Web Frontend

### Goal

Add a responsive server-rendered Razor Pages frontend that presents the same
verified application behavior.

### Required outcome

- The web project targets .NET 10 and references the existing StormWatch
  application.
- It reuses `StormWatchDataService` and `StormRiskService`.
- It displays the resolved location, source, five forecast periods, risk score,
  peak, evidence, and disclaimer.
- Blank input and expected failures are handled without exposing a stack trace.
- Cancellation follows the browser request lifetime.
- OpenWeather credentials and requests remain server-side.
- The page uses semantic accessible HTML and works at narrow and desktop widths.
- Browser traffic remains on localhost and contains no API key.

### Completion evidence

- The web project builds while the original 17 tests remain green.
- Fixture mode renders the same deterministic Bengaluru assessment as the CLI.
- Accessibility, narrow-width layout, failure handling, and browser network
  checks pass.

## Stretch Goals

1. Add metric and imperial display while keeping scoring internally metric.
2. Add a structured tool that compares two cities deterministically.
3. Return the top three risk windows.
4. Cache successful forecasts for five minutes without caching failures.
5. Add MCP SDK client integration tests to the participant project.

## Troubleshooting

Use [TROUBLESHOOTING.md](TROUBLESHOOTING.md) only for environment, restore,
network, fixture, or MCP startup failures. Implementation decisions remain part
of the challenge.

## Official References

- [Develop with the MCP C# SDK](https://learn.microsoft.com/dotnet/ai/get-started-mcp)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [Add and manage MCP servers in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers)
- [Razor Pages architecture and concepts](https://learn.microsoft.com/aspnet/core/razor-pages/)
- [OpenWeather Geocoding API](https://openweathermap.org/api/geocoding-api)
- [OpenWeather 5 Day / 3 Hour Forecast](https://openweathermap.org/forecast5)
