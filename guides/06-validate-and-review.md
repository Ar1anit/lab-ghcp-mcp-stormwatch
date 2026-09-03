# Task 6: Validate and Review

**Timebox:** 6 minutes

**Clock:** 0:54-1:00
**Code changes:** Only if validation exposes a concrete defect

## Outcome

Produce credible end-to-end evidence, approve one MCP invocation deliberately,
inspect the final diff, and distinguish what the lab proves from what remains a
limitation.

## Step 1: Run the Full Automated Suite

Run:

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```

All 17 tests must pass. A passing subset is not sufficient because the CLI and
MCP layers compose the weather and risk boundaries.

## Step 2: Run the Deterministic CLI Acceptance Path

Run:

```powershell
dotnet run --project .\stormwatch -- Bengaluru --fixture .\tests\fixtures\forecast.json
```

Inspect the output rather than checking only the exit code. Confirm:

- location is `Bengaluru, Karnataka, IN`;
- exactly five forecast rows are shown;
- result is `WARNING (100/100)`;
- peak is `2026-08-30 06:00 UTC`;
- all four peak indicators are explained; and
- the educational and non-official warning is present.

## Step 3: Write and Approve One MCP Invocation Request

Write your own request in Copilot Agent mode. It should require current
StormWatch evidence for Bengaluru, make the expected claims inspectable, and
retain the educational-use qualification. Do not name a tool unless you intend
to test explicit tool selection; observe whether your wording leads Copilot to
choose the appropriate StormWatch capability.

Before approving the proposed call:

1. Confirm the selected tool is StormWatch `assess_storm_risk`.
2. Confirm the only argument is the intended city.
3. Confirm no unrelated tool or file operation is requested.
4. Approve the call.

For reproducible workshop evidence, fixture mode should return the same peak and
score as the CLI. Live mode may validly return another level.

Inspect the raw structured result if the client exposes it. The answer should be
grounded in the returned fields and retain the disclaimer.

## Step 4: Review the Final Change Set

Run:

```powershell
git -c core.whitespace=cr-at-eol diff --check
git diff --name-only
git diff -- .\stormwatch
```

Expected implementation files are:

- `stormwatch/Weather.cs`
- `stormwatch/Risk.cs`
- `stormwatch/StormWatchApp.cs`
- `stormwatch/StormWatchTools.cs`

Investigate any changed test, model, project, configuration, fixture, or
instruction file. Such a change may be valid, but it requires a specific reason
and should not merely make a failing implementation pass.

Write your own focused review prompt. Bound it to the completed project and
requirements, request concrete file-grounded findings in the relevant quality
categories, and require a clear response when no finding exists. Verify each
finding against the code before acting.

## Step 5: Validate Through the Foundry Model

Place the private `.env` supplied by the facilitator in the `starter/`
directory. Do not open it in Copilot Chat, commit it, print it, or include it in
a screenshot. It provides the dedicated workshop endpoint, deployment name,
and temporary API key. Then run:

```powershell
dotnet run --project .\StormWatch.FoundryClient -- `
      .\stormwatch\StormWatch.csproj `
      .\tests\fixtures\forecast.json `
      Bengaluru
```

Confirm that the client discovers both local tools and that the response is
grounded in a local tool result. This model deployment is separate from the
model used by GitHub Copilot.

## Step 6: State What the Evidence Does Not Prove

Choose at least one limitation and explain it:

- the thresholds are an educational rule set, not a meteorological model;
- mocked HTTP tests do not prove OpenWeather availability or future payload
  compatibility;
- one successful MCP call does not prove authorization or policy suitability
  for every environment;
- a passing fixture does not validate every city, locale, or malformed payload;
- prerelease MCP SDK behavior can change after a package update; or
- local MCP servers run with the user's permissions and therefore still require
  careful tool approval.

## Completion Gate

You can say **“The 60-minute StormWatch lab is done”** only when all of the
following are true:

- [ ] All 17 automated tests pass in one full-suite run.
- [ ] The deterministic fixture CLI produces five rows, `WARNING (100/100)`,
      peak `2026-08-30 06:00 UTC`, evidence, and the safety disclaimer.
- [ ] VS Code advertises both MCP tools, and one deliberately reviewed
      `assess_storm_risk` invocation completes in Copilot Agent mode.
- [ ] The Foundry-backed local client discovers both tools and produces a
      fixture-grounded response using the facilitator-provided model.
- [ ] The MCP result is structured, matches the selected data source, and is not
      presented as an official or reliable prediction.
- [ ] The final diff is free of whitespace errors and contains only changes you
      can justify; tests were not weakened to obtain a pass.
- [ ] No OpenWeather key appears in source, configuration, chat, commands, logs,
      exceptions, screenshots, Git history, or tool output.
- [ ] The Foundry workshop key remains only in the ignored `.env` file and does
      not appear in source, chat, commands, logs, screenshots, or Git history.
- [ ] Weather transport, scoring, CLI rendering, and MCP transport remain
      separate and compose through the existing records and services.
- [ ] You can explain what each verification step proves and name at least one
      important limitation that remains.
- [ ] You can explain why MCP tool approval is still a security decision even
      when the server and tool were created locally.

**Final evidence set:** the 17-test summary, fixture CLI output, discovered tool
schemas, one inspected Copilot MCP result, one Foundry-backed result, the
reviewed diff, and one stated limitation.

Continue to [Advanced Task 7: Build a web frontend](07-build-frontend.md), or
return to the [participant guide index](README.md) if you are completing only
the 60-minute core.
