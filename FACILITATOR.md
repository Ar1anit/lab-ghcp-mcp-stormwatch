# StormWatch Facilitator Guide

Use this guide to deliver the 60-minute C# and MCP core followed by the
20-minute frontend task. The complete seven-task route takes 80 minutes and
teaches disciplined agent-assisted development, MCP integration, and reuse
across presentation layers—not weather science.

## Room Setup

Complete this at least one day before delivery:

- Verify the .NET 10 SDK and C# Dev Kit on the standard developer image.
- Confirm the ASP.NET Core Web App (`webapp`) template is available.
- Verify GitHub Copilot Agent mode and local MCP servers are allowed by policy.
- Have participants activate OpenWeather keys in advance.
- Test Direct Geocoding and 5 Day / 3 Hour Forecast on the venue network.
- Open `starter/` as its own workspace and review `.vscode/mcp.json`.
- Run the reference acceptance suite and protocol smoke:

```powershell
dotnet test .\solution\StormWatch.Tests\StormWatch.Tests.csproj
dotnet run --project .\solution\StormWatch.Smoke -- `
  .\solution\StormWatch\StormWatch.csproj `
  .\starter\tests\fixtures\forecast.json
```

- Rehearse the offline fallback in `TROUBLESHOOTING.md`.
- Keep `solution/` closed unless recovery is needed.

Do not introduce patient, product, operational, or restricted data.

## Core Definition Of Done

- All 17 tests pass.
- The fixture CLI reports `WARNING (100/100)` at `2026-08-30 06:00 UTC`.
- No API key appears in code, commands, chat, logs, or output.
- The official C# SDK server advertises both tools over stdio.
- One MCP call completes in Copilot Agent mode.
- Output is labeled as an educational heuristic.

## Frontend Definition Of Done

- The Razor Pages project builds and the original 17 tests remain unchanged.
- Fixture mode renders five periods and the expected Bengaluru assessment.
- The web page reuses `StormWatchDataService` and `StormRiskService`.
- Blank input, accessibility, and narrow-width checks pass.
- Browser traffic stays on localhost and contains no credential or stack trace.

## Delivery Clock

| Time | Facilitator action | Participant checkpoint |
| --- | --- | --- |
| 0:00-0:07 | Frame scenario, run red baseline, identify boundaries. | Intentional TODO failures only. |
| 0:07-0:20 | Coach `HttpClient` adapter and JSON review. | Six weather tests pass. |
| 0:20-0:35 | Treat thresholds as requirements and implement risk. | Eight risk tests pass. |
| 0:35-0:44 | Complete CLI and inspect claims. | All 17 tests and fixture CLI pass. |
| 0:44-0:54 | Explain SDK registration, attributes, and stdio. | Two tools appear in Copilot. |
| 0:54-1:00 | Approve one invocation and review. | Names a limitation and guardrail. |
| 1:00-1:20 | Scaffold Razor Pages and coach service reuse, browser trust boundaries, and responsive review. | Deterministic frontend result passes its completion gate. |

Protect the final 16 minutes. Invoking a self-built official-SDK tool is the
core GHCP x MCP outcome. Reserve the additional 20 minutes when Task 7 is part
of the delivery.

## Teaching Notes

### Vocabulary

| Term | In this lab |
| --- | --- |
| MCP host/client | VS Code and GitHub Copilot Agent mode |
| MCP server | `dotnet run --project StormWatch -- --mcp` |
| MCP C# SDK | `ModelContextProtocol` NuGet package |
| MCP tools | `get_forecast`, `assess_storm_risk` |
| Application core | records, adapter, risk service, CLI |

### Weather Adapter

Participants may use forecast-by-city. Point to the test requiring Direct
Geocoding first and coordinates second. Keep `HttpClient` injected so request
construction is testable without a network.

### Risk Rules

The peak fixture is $60 + 25 + 20 + 15 = 120$, capped at 100. Tiered indicators
are exclusive: 15 m/s earns 25 wind points, not 40.

### MCP SDK

Show the registration chain in `Program.cs` and attributes in
`StormWatchTools.cs`. Tool discovery comes from SDK assembly scanning; the
methods return records so the SDK emits structured results.

Stdio reserves stdout for protocol messages. The starter clears logging
providers before running MCP mode. Local servers execute with the user's
permissions, so pause on the proposed tool name and city before approval.

### Frontend

Keep the browser thin and server-rendered. The Razor Page should inject
`StormWatchDataService`, pass `HttpContext.RequestAborted`, call
`StormRiskService.Assess`, and render typed values. It must not parse the CLI
string or call OpenWeather from JavaScript.

Run the web host separately from the MCP process. ASP.NET logging is normal in
the web process but must never be introduced into the MCP stdio stream. Use the
fixture for the acceptance path and inspect browser developer tools to prove
that no key or direct OpenWeather request crosses the browser boundary.

## Progressive Hints

### Weather

1. Parse the first geocoding array element into `Location`.
2. Build encoded query pairs with `Uri.EscapeDataString`.
3. Use a linked `CancellationTokenSource` and `CancelAfter`.
4. Catch HTTP, timeout, and JSON errors without including request URLs.

### Risk

1. Score one point first, then order by score descending and time ascending.
2. Use `if`/`else if` for each tier.
3. Cap only after collecting every reason.

### CLI

1. Fixture mode returns before reading the environment.
2. Catch expected exceptions, write to stderr, return 2.
3. Render with `Take(5)` and invariant numeric formatting.

### MCP

1. Await `dataService.LoadAsync` in each tool.
2. Map domain values into the supplied structured result records.
3. Call `StormRiskService.Assess`; do not repeat thresholds.

### Frontend

1. Reference `stormwatch/StormWatch.csproj` from the Razor Pages project.
2. Register and inject `StormWatchDataService`.
3. Keep forecast and assessment as typed page-model properties.
4. Use the fixture and localhost before troubleshooting live weather.
5. Check mobile reflow and browser network traffic before polishing visuals.

## Recovery

Give one hint at a time. If less than 16 minutes remain, restore the C# source:

```powershell
Copy-Item -Force ..\solution\StormWatch\*.cs .\StormWatch\
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```

If live API access fails, use fixture MCP mode from `TROUBLESHOOTING.md`.

## Debrief

- Which context made Copilot's code easier to verify?
- What belongs in the application core rather than an MCP wrapper?
- What does the SDK automate, and what security decisions remain yours?
- What did the tests prove, and what did they not prove?
- Which responsibilities must remain server-side when the application gains a
  browser frontend?