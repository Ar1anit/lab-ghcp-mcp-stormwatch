# Task 1: Orient and Verify

**Timebox:** 7 minutes

**Clock:** 0:00-0:07
**Code changes:** None

## Outcome

Establish a trustworthy starting point. You should know what the application is
supposed to do, where each responsibility belongs, which failures are
intentional, and which failures indicate a setup problem.

Do not implement a TODO during this task.

## Step 1: Confirm the Workspace

Open `starter/` as the VS Code workspace root. The Explorer should show:

```text
.github/
.vscode/
StormWatch.Tests/
stormwatch/
tests/
```

Open `.github/copilot-instructions.md` and identify the constraints that apply to
all later tasks:

- .NET 10 and nullable reference types;
- separate transport, risk, CLI, and MCP responsibilities;
- injected `HttpClient`;
- no API key in code, chat, logs, or output;
- synthetic or mocked tests only; and
- no application output on stdout in MCP mode.

## Step 2: Verify the Toolchain

Run:

```powershell
dotnet --version
dotnet --list-sdks
```

The active version should be .NET 10, and the SDK list must include a `10.x`
entry. A runtime without the SDK is not sufficient.

Restore dependencies:

```powershell
dotnet restore .\StormWatch.Tests\StormWatch.Tests.csproj
```

Package restore must complete before you interpret test failures.

## Step 3: Establish the Red Baseline

Run:

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```

The starter contains 17 tests and four intentional implementation checkpoints:

| TODO | File | Responsibility |
| --- | --- | --- |
| 1 | `stormwatch/Weather.cs` | HTTP requests and JSON parsing |
| 2 | `stormwatch/Risk.cs` | Deterministic risk scoring |
| 3 | `stormwatch/StormWatchApp.cs` | CLI orchestration and rendering |
| 4 | `stormwatch/StormWatchTools.cs` | MCP tool wrappers |

Failures caused by `NotImplementedException` in these TODO files are expected.
Compiler errors, missing fixtures, package errors, and test discovery failures
are not expected. Resolve those as environment problems before continuing.

## Step 4: Build the Architecture Map

Read:

- `README.md`, especially architecture and risk thresholds;
- `stormwatch/Models.cs`;
- `stormwatch/Program.cs`;
- the three test classes; and
- the four TODO files.

Write your own prompt asking Copilot to analyze the selected context without
editing. Your prompt should make the expected analysis clear enough that you
can check whether it identifies the four implementation checkpoints, the
security constraints, and the boundary between application logic and MCP
transport.

Review the response against the source. Copilot should not move scoring into the
MCP methods or suggest live API calls from tests.

## Architecture You Should Be Able to Explain

```text
city
  -> OpenWeather adapter
  -> immutable Forecast records
  -> risk service
  -> CLI report
  -> thin MCP tools over the same application services
```

The MCP layer is an adapter, not a second implementation of weather or risk
logic.

## If You Are Stuck

- If restore fails, use the package steps in
  [`TROUBLESHOOTING.md`](../TROUBLESHOOTING.md#restore-or-package-errors).
- If tests do not compile, verify the .NET 10 SDK and workspace root.
- If tests unexpectedly pass, confirm you opened the untouched `starter/`
  project rather than `solution/`.

## Completion Gate

You can say **“Task 1 is done”** only when all of the following are true:

- [ ] `dotnet --version` reports .NET 10 and restore succeeds.
- [ ] The test project is discovered and the only failing behavior traces to the
      four intentional TODO checkpoints—not missing packages, fixtures, or
      compiler setup.
- [ ] You can name the owner of weather transport, risk scoring, CLI rendering,
      and MCP transport without looking at the architecture diagram.
- [ ] You can explain why tests use a fixture or mocked `HttpClient` instead of
      the live OpenWeather API.
- [ ] You can explain why a discovered MCP tool can still fail at runtime while
      its TODO body throws `NotImplementedException`.
- [ ] No source or test file was modified during orientation.
- [ ] You have identified the credential rule and the stdio rule that must remain
      true throughout the lab.

**Evidence to retain:** the restore result, the red test summary, and your
four-part architecture map.

Continue to [Task 2: Build the weather adapter](02-build-weather-adapter.md).
