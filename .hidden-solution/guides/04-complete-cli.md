# Task 4: Complete the CLI

**Timebox:** 9 minutes

**Clock:** 0:35-0:44
**Primary file:** `stormwatch/StormWatchApp.cs`

## Outcome

Complete a CLI that can load either a deterministic fixture or live
OpenWeather data, assess the forecast with the existing risk service, render a
concise report, and handle expected user or service errors without a stack trace
or credential leak.

## Step 1: Select the CLI Boundary

Open:

- the CLI section in `README.md`;
- `stormwatch/StormWatchApp.cs`;
- `stormwatch/Weather.cs`;
- `stormwatch/Risk.cs`;
- `StormWatch.Tests/AppTests.cs`; and
- `tests/fixtures/forecast.json`.

The CLI composes existing services. It should not repeat JSON parsing or risk
thresholds.

## Step 2: Ask Copilot for the CLI

Use:

```text
Implement only stormwatch/StormWatchApp.cs. Accept <city> [--fixture <path>].
Fixture mode must not require OPENWEATHER_API_KEY. Print five forecast points,
peak level, score, time, evidence, and the educational disclaimer. Handle
expected errors with exit code 2, stderr, and no stack trace or secret. Compose
the existing parser and risk service; do not duplicate their logic. Run
AppTests, then all tests.
```

## Step 3: Review Argument and Data-Source Handling

`RunAsync` should:

- accept exactly `<city>` or `<city> --fixture <path>`;
- show a short usage message for invalid arguments;
- write normal reports to the supplied output writer;
- write expected errors to the supplied error writer;
- return `0` for success and `2` for expected usage or service failures; and
- preserve cancellation rather than converting it into a false success.

`LoadForecastAsync` should:

1. use the fixture immediately when a fixture path is supplied;
2. avoid reading `OPENWEATHER_API_KEY` in fixture mode;
3. require a non-empty key only for live mode; and
4. call the existing `OpenWeatherClient` rather than reproducing transport code.

The key must come from the environment and must never be printed.

## Step 4: Review the Report

`RenderReport` should contain:

- the resolved location;
- exactly the next five forecast points;
- UTC timestamps;
- temperature, description, wind, rain, and pressure with units;
- uppercase level and numeric score;
- peak UTC time;
- every evidence reason, or an explicit no-indicator message; and
- a clear statement that the signal is an educational heuristic and not an
  official warning or reliable storm prediction.

Use invariant formatting for decimal numbers so the report remains stable
across locales.

## Step 5: Run Focused Tests

Run:

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj --filter FullyQualifiedName~AppTests
```

The three application tests prove the report shape, offline execution, and
missing-key error path.

## Step 6: Run the Integrated Fixture

Run all tests, then the CLI:

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
dotnet run --project .\stormwatch -- Bengaluru --fixture .\tests\fixtures\forecast.json
```

The fixture should report:

- five UTC forecast rows;
- `WARNING (100/100)`; and
- peak time `2026-08-30 06:00 UTC`.

Do not use live weather as the acceptance result; it changes over time.

## Step 7: Inspect the Diff

Run:

```powershell
git diff -- .\stormwatch\StormWatchApp.cs
git diff --name-only
```

Check that tests were not changed and the CLI did not absorb weather or scoring
logic.

## If You Are Stuck

1. Return from the fixture branch before reading the environment.
2. Keep output and error writers injectable so tests can inspect them.
3. Catch only expected I/O, argument, and weather-service failures.
4. Render forecast rows with `Take(5)`.
5. Keep the disclaimer in the report for every assessment level.

## Completion Gate

You can say **“Task 4 is done”** only when all of the following are true:

- [ ] All three `AppTests` pass, followed by all 17 tests.
- [ ] Fixture mode succeeds with no API key and does not touch the live network.
- [ ] The fixture output contains five forecast rows, `WARNING (100/100)`, the
      expected peak time, evidence, units, and the full safety disclaimer.
- [ ] Invalid arguments and a missing live API key produce exit code 2, write to
      stderr, leave stdout clean, and do not print a stack trace.
- [ ] The CLI composes `WeatherParser`/`OpenWeatherClient` and
      `StormRiskService`; it does not duplicate their rules.
- [ ] Decimal and timestamp rendering is deterministic across machine locales.
- [ ] No credential value is present in source, output, errors, or the command
      history used for this task.
- [ ] You can explain why fixture output is stronger acceptance evidence than a
      successful live-weather run.

**Evidence to retain:** the 17-test pass summary and the complete fixture report
showing the expected score, peak, evidence, and disclaimer.

Continue to [Task 5: Expose MCP tools](05-expose-mcp-tools.md).
