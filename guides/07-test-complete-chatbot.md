# Task 7: Test the Complete Chatbot

**Timebox:** 5 minutes  
**Clock:** 1:25-1:30  
**Code changes:** Only if evidence exposes a concrete defect

## Outcome

Produce credible end-to-end evidence for the completed chatbot and distinguish
what the lab proves from what remains uncertain.

## Step 1: Run All Automated Tests

```powershell
dotnet test .\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```

All 22 tests must pass. The tests use fake chat/embedding boundaries and mocked or
fixture weather data; they must not call live services.

## Step 2: Run the Deterministic Integrated Path

```powershell
dotnet run --project .\StormWatch.Chat -- --with-tools `
  --fixture .\tests\fixtures\forecast.json
```

Check three turns:

1. a corpus question produces cited local source files;
2. a Bengaluru forecast/risk question invokes MCP and reports the fixture
   result; and
3. a follow-up refers naturally to an earlier turn.

Do not treat a fluent response as evidence. Tie each knowledge claim to a
retrieved source and each weather claim to tool output.

## Step 3: Review Security and Limits

Run:

```powershell
git diff --check
git diff --name-only
git status --short
```

Confirm `.env` and `.env.openweather` are absent. Review every
changed file and check that tests were not weakened.

Name at least one remaining limitation:

- local source files can still contain stale, incomplete, or malicious text;
- vector similarity does not prove semantic relevance;
- citations show supplied sources, not independent truth;
- mocked tests do not prove future Azure or OpenWeather availability;
- the storm thresholds are an educational rule set, not weather science; or
- one successful tool call does not prove production authorization or safety.

## Completion Gate

- [ ] All 22 tests pass across both projects.
- [ ] The integrated chatbot demonstrates cited RAG, MCP weather, and retained
      history.
- [ ] Unsupported claims produce bounded uncertainty rather than invention.
- [ ] No secret appears in tracked files, output, logs, screenshots, or Git
      history.
- [ ] The final diff is understood and free of whitespace errors.
- [ ] You can explain one limitation that the evidence does not remove.

**Evidence to retain:** two green test summaries, a redacted three-turn
transcript, discovered tools, and your limitation statement.

Continue to [Optional Task 8: Add a web chat interface](08-add-web-chat-interface.md),
or stop with the verified console assistant.
