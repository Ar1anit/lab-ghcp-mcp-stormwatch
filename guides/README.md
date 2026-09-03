# StormWatch Participant Guides

These guides divide the lab into a 60-minute core and an optional 20-minute
advanced frontend task. Work from the `starter/` directory and keep this index
open so you can move on when a task's completion gate is satisfied.

## 60-Minute Core Route

| Clock | Timebox | Guide | Outcome |
| --- | ---: | --- | --- |
| 0:00-0:07 | 7 min | [1. Orient and verify](01-orient-and-verify.md) | Confirm the environment, red baseline, and architecture boundaries. |
| 0:07-0:20 | 13 min | [2. Build the weather adapter](02-build-weather-adapter.md) | Convert mocked OpenWeather responses into typed forecast records. |
| 0:20-0:35 | 15 min | [3. Implement storm-risk rules](03-implement-risk-rules.md) | Produce deterministic, explainable risk assessments. |
| 0:35-0:44 | 9 min | [4. Complete the CLI](04-complete-cli.md) | Run from a city or fixture and render a safe report. |
| 0:44-0:54 | 10 min | [5. Expose MCP tools](05-expose-mcp-tools.md) | Publish application capabilities through the official MCP SDK. |
| 0:54-1:00 | 6 min | [6. Validate and review](06-validate-and-review.md) | Prove the integrated outcome and name its limits. |

The timeboxes total exactly 60 minutes. Move forward when the task is sound
rather than polishing nonessential details.

## Advanced Frontend Task

| Clock | Timebox | Guide | Outcome |
| --- | ---: | --- | --- |
| 1:00-1:20 | 20 min | [7. Build a web frontend (advanced)](07-build-frontend.md) | Add a responsive Razor Pages presentation layer without duplicating application logic or exposing credentials. |

The complete seven-task route takes 80 minutes. Task 6 remains the completion
gate for the original CLI and MCP core; advanced Task 7 optionally extends that
verified application through a separate web host.

## How to Use Each Guide

Each task follows the same pattern:

1. Read the outcome and constraints.
2. Give Copilot only the listed working context.
3. Write your own prompt for the task. Include the intended outcome, change
  boundary, constraints, and verification you expect Copilot to perform.
4. Inspect the proposed change before accepting it.
5. Run the focused verification command.
6. Evaluate every item in the completion gate.

The guides deliberately do not provide copy-ready prompts. Prompt design is
part of the exercise: decide what context Copilot needs, make the requested
scope testable, and refine your prompt when the resulting evidence exposes a
gap.

The completion gates are deliberately stronger than “the file exists.” They ask
whether the behavior is correct, the design boundary is preserved, security
constraints hold, and you can explain the evidence.

## Working Rules

- Do not modify tests to make generated code pass.
- Never paste or commit an OpenWeather API key.
- Use the synthetic fixture and mocked HTTP tests for deterministic evidence.
- Keep weather transport, scoring, CLI rendering, and MCP transport separate.
- Treat the frontend as a presentation adapter over the existing services.
- Run focused tests while iterating, then the full suite before finishing.
- Review Copilot edits and tool calls before accepting or approving them.
- Treat StormWatch output as an educational heuristic, never an official
  warning or reliable storm prediction.

## Time Recovery

If you fall behind, prefer a smaller correct result over broad unfinished edits.
Ask for one hint at a time from the facilitator.

If fewer than 16 minutes remain before the MCP task, the facilitator may restore
the reference application source so you can still complete the core MCP
experience:

```powershell
Copy-Item -Force ..\solution\stormwatch\*.cs .\stormwatch\
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```

If live OpenWeather access is unavailable, use the fixture configuration in
[`TROUBLESHOOTING.md`](../TROUBLESHOOTING.md#offline-mcp-fallback).

## Core Lab Outcome

At the end of 60 minutes, you should have evidence that:

- all 17 tests pass;
- fixture mode reports `WARNING (100/100)` with the expected peak;
- no credential appears in source, chat, logs, commands, or output;
- the SDK advertises `get_forecast` and `assess_storm_risk`;
- one MCP invocation returns structured data in Copilot Agent mode; and
- the Foundry-backed local client can offer the same tools to the shared model;
  and
- you can explain at least one limitation that tests do not remove.

## Frontend Outcome

After Task 7, you should also have evidence that:

- a responsive Razor Pages frontend renders the deterministic fixture result;
- browser code never calls OpenWeather or receives the API key;
- the web page reuses the existing data and risk services; and
- accessibility, negative-input, mobile-width, and secret-boundary checks pass.
