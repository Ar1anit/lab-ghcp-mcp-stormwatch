# Optional Task 8: Add a Web Chat Interface

**Timebox:** 20 minutes  
**Clock:** 1:30-1:50  
**Level:** Advanced

## Outcome

Add a responsive, server-rendered Razor Pages interface over the completed
conversation service. The browser displays chat turns and citations but never
receives model, embedding, or OpenWeather credentials.

## Step 1: Preserve the Boundary

Create `StormWatch.Web` beside the existing projects:

```powershell
dotnet new webapp -n StormWatch.Web
dotnet add .\StormWatch.Web\StormWatch.Web.csproj reference `
  .\StormWatch.Chat\StormWatch.Chat.csproj
```

Refactor only as needed to register `IChatSession` server-side. Do not call
Foundry, the embedding model, OpenWeather, or MCP from browser JavaScript. Do not
parse console output or create a second RAG pipeline.

## Step 2: Build the Chat Page

The page should provide:

- a labeled message input and clear send command;
- an ordered transcript with visually distinct user and assistant turns;
- relative source labels associated with the assistant answer that used them;
- validation and safe service-error states;
- keyboard focus returned to the input after a response;
- an `aria-live` region for new assistant messages; and
- a narrow-width layout with no horizontal scrolling.

Keep display text concise. Never render raw prompts, hidden retrieved context,
tool schemas, stack traces, or environment values.

## Step 3: Manage Conversation State

Use a scoped server-side chat session per browser session. Do not use a
singleton conversation history across participants. For this lab, memory may be
lost when the process restarts; persistent chat storage is out of scope.

Limit submitted text length and retain only the turns needed for the exercise.
Do not log message bodies because workshop questions may be sent to both Search
and the Foundry model.

## Step 4: Validate

```powershell
dotnet build .\StormWatch.Web\StormWatch.Web.csproj
dotnet test .\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
dotnet run --project .\StormWatch.Web\StormWatch.Web.csproj
```

In a desktop and mobile-width browser, verify a cited knowledge turn, a
fixture-backed weather turn, a follow-up, blank input, keyboard flow, source
labels, and a simulated service failure. Inspect Network and page source to
confirm that no browser request goes directly to Foundry or
OpenWeather and that no key crosses the browser boundary.

## Completion Gate

- [ ] The web project builds and all 22 original tests remain green.
- [ ] Chat history is isolated per browser session.
- [ ] Citations remain attached to the answer that used them.
- [ ] Blank and oversized input receive accessible validation.
- [ ] Desktop and narrow-width layouts have no overlap or horizontal scroll.
- [ ] Browser traffic contains no direct Azure or OpenWeather request and no
      credential.
- [ ] The web layer reuses the same chat, retrieval, grounding, and MCP services.

**Evidence to retain:** green builds/tests, desktop and mobile screenshots, and
a browser network inspection showing the credential boundary.
