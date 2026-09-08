# StormWatch Facilitator Guide

Deliver a 90-minute required lab followed by an optional 20-minute web-chat
task. Participants progressively build one assistant: multi-turn chat, local
vector retrieval, grounded answers, weather/risk services, then MCP tools.

## Room Setup

Complete this at least one day before delivery:

- Verify the .NET 10 SDK, C# Dev Kit, GitHub Copilot Chat, and Agent mode on the
  participant image.
- Prepare a dedicated Foundry resource with a tool-capable chat deployment, an
  embedding deployment, and conservative quota for the expected concurrency.
- Prepare the same approved educational `.md` and `.txt` corpus in `rag-data`
  for every participant.
- Keep the corpus within 100 files, 512,000 bytes per file, and 500 chunks with
  the supplied defaults.
- Record one known-good corpus question and one unsupported question for the
  live RAG gate.
- Activate and test a temporary OpenWeather key for Direct Geocoding and the 5
  Day / 3 Hour Forecast APIs.
- Confirm local MCP servers are allowed by organizational policy.
- Confirm outbound HTTPS access to Foundry and
  `api.openweathermap.org`.
- Prepare the two ignored participant files described in [FOUNDRY.md](../FOUNDRY.md):
  `.env` and `.env.openweather`, plus `rag-data`.
- Rehearse model-only mode, live RAG, fixture-backed chatbot tools, Copilot MCP
  discovery, and the fallback paths.
- Keep `solution/` closed unless recovery is required.

Do not introduce patient, product, operational, or other restricted data.
Corpus chunks and questions are sent to the embedding deployment; questions,
retrieved passages, tool arguments, and tool results may be sent to the chat
model.

## Reference Validation

From the repository root:

```powershell
dotnet test .\solution\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj
dotnet test .\solution\StormWatch.Tests\StormWatch.Tests.csproj
dotnet run --project .\solution\StormWatch.Smoke -- `
  .\solution\stormwatch\StormWatch.csproj `
  .\starter\tests\fixtures\forecast.json
```

Expected automated evidence:

- 5 chatbot/RAG tests pass;
- 17 weather/application tests pass; and
- the protocol smoke discovers and invokes exactly `get_forecast` and
  `assess_storm_risk`.

With the two private files and `rag-data` in `starter/`, run the complete
reference chatbot from that working directory so it can load them without
copying secrets:

```powershell
Push-Location .\starter
dotnet run --project ..\solution\StormWatch.Chat -- --no-rag
dotnet run --project ..\solution\StormWatch.Chat
dotnet run --project ..\solution\StormWatch.Chat -- --with-tools `
  --server-project ..\solution\stormwatch\StormWatch.csproj `
  --fixture .\tests\fixtures\forecast.json
Pop-Location
```

The second command must answer the known-good question with sources and bound
the unsupported question. The third must invoke a local MCP tool and report the
fixture's `WARNING (100/100)` result.

## Core Definition of Done

- All 22 tests pass.
- A two-turn model-only conversation retains history.
- Local retrieval returns at most three mapped passages through a fake
  embedding boundary.
- A supported knowledge answer cites only sources supplied for that turn.
- An unsupported answer does not invent or reuse stale sources.
- The weather fixture reports `WARNING (100/100)` at
  `2026-08-30 06:00 UTC`.
- The local server advertises exactly two structured tools.
- The chatbot and GitHub Copilot both invoke the same MCP server.
- Model and weather credentials remain in separate ignored files and reach only
  the processes that require them.
- Every storm-risk result remains labeled as an educational heuristic.

## Optional Web Definition of Done

- A server-rendered Razor Pages chat UI reuses the existing assistant services.
- Conversation state is scoped per browser session, never global.
- Citations stay attached to the assistant turn that used them.
- Blank and oversized input have accessible validation.
- Desktop and mobile-width layouts have no overlap or horizontal scroll.
- Browser traffic contains no direct Foundry or OpenWeather call and no
  credential.
- All 22 original tests remain green.

## Delivery Clock

| Time | Facilitator focus | Participant checkpoint |
| --- | --- | --- |
| 0:00-0:08 | Frame one assistant with separate model, retrieval, grounding, and tool boundaries. | 22 tests discovered; failures map to numbered TODOs. |
| 0:08-0:22 | Coach chat history and console-loop behavior. | Two model-only chat tests and live two-turn chat pass. |
| 0:22-0:37 | Explain chunking, embeddings, cosine similarity, and the fake embedding boundary. | Two local-RAG tests pass; raw retrieval remains separate from generation. |
| 0:37-0:49 | Review prompt-injection boundary, no-evidence behavior, and source lists. | All five chatbot tests and one cited live answer pass. |
| 0:49-1:11 | Coach HTTP parsing and deterministic risk thresholds. | All 17 domain/application tests and fixture CLI pass. |
| 1:11-1:25 | Explain MCP registration, stdio, discovery, and tool approval. | Chatbot and Copilot both invoke the two local tools. |
| 1:25-1:30 | Run final tests and evidence review. | 22 tests, three-turn transcript, and limitation statement pass. |
| 1:30-1:50 | Optional Razor Pages chat adapter. | Responsive, accessible, credential-free browser path passes. |

Protect the final 19 minutes. The distinctive outcome is not merely creating an
MCP server; it is showing one assistant use retrieved knowledge and local tools
for different kinds of claims.

## Teaching Notes

### Conversation

Task 2 runs with `--no-rag`. Prior natural user and assistant turns belong in
history. Augmented prompts do not: storing them would mix transient retrieved
passages into future user intent.

### Retrieval and Grounding

Retrieval is not generation. Task 3 returns typed passages and is tested through
`ITextEmbeddingService`. Task 4 treats those passages as untrusted data, assigns
stable `[S#]` identifiers, and appends deterministic sources.

Do not imply that citations prove truth. They prove which supplied text was
available to the model. The unsupported-question gate is as important as the
known-good answer.

### Weather and Risk Hints

Weather requests geocode first and forecast by coordinates. Tests use injected
`HttpClient`; acceptance uses the synthetic fixture. The peak fixture scores
$60 + 25 + 20 + 15 = 120$, capped at 100. Tiered indicators are exclusive.

### MCP Hints

`StormWatchTools` must remain thin. The chatbot starts the same local stdio
server that VS Code uses, discovers exactly two tools, and gives them to
`Microsoft.Extensions.AI` function invocation. Local retrieval is not exposed
as an MCP tool in this lab; it is an explicit RAG application boundary.

### Credential Isolation

- `.env` reaches the chatbot model client.
- The embedding deployment in `.env` is used only when RAG is enabled.
- `.env.openweather` reaches the MCP server only in live weather mode.

Never combine these files for convenience. Participant-written MCP code should
not inherit the Foundry key or embedding deployment configuration.

### Optional Web Chat

The browser is a presentation adapter. Calls to models and MCP remain
server-side. Use scoped conversation state and avoid a singleton transcript
shared across participants.

## Progressive Hints

### Chatbot

1. Build each request from the system message plus prior successful turns.
2. Store the natural question, not the grounded prompt.
3. Treat `/exit` and EOF as normal completion.

### Local Vector Retrieval

1. Embed loaded chunks once when constructing the index.
2. Normalize both document and query vectors before taking their dot product.
3. Sort by similarity descending, then passage ID for deterministic ties.

### Grounding

1. Number passages before generating.
2. State that retrieved content is data, not instructions.
3. Append sources deterministically after model completion.

### Weather and Risk

1. Parse geocoding before forecast payloads.
2. Use `if`/`else if` for tiered thresholds.
3. Score first, then order by score descending and time ascending.

### MCP

1. Compose `StormWatchDataService` and `StormRiskService`.
2. In fixture mode, pass only `STORMWATCH_FIXTURE_PATH` to the child process.
3. Reject unexpected discovered tool sets.

## Recovery

Give one hint at a time. If a task must be restored from `starter/`:

```powershell
# Chat/RAG recovery
Copy-Item -Force ..\solution\StormWatch.Chat\*.cs .\StormWatch.Chat\

# Weather/risk recovery
Copy-Item -Force ..\solution\stormwatch\Weather.cs .\stormwatch\
Copy-Item -Force ..\solution\stormwatch\Risk.cs .\stormwatch\

# MCP recovery
Copy-Item -Force ..\solution\stormwatch\StormWatchTools.cs .\stormwatch\
Copy-Item -Force ..\solution\StormWatch.Chat\McpToolSession.cs .\StormWatch.Chat\
```

Re-run the relevant focused tests after any recovery. If a live service fails,
use the mode-specific fallback in [TROUBLESHOOTING.md](../TROUBLESHOOTING.md)
without claiming missing evidence.

## Debrief

- What is the difference between conversation memory and retrieved context?
- What does a source list prove, and what does it not prove?
- Which claims should come from local files and which should come from MCP tools?
- Why should the MCP child process not inherit model or embedding configuration?
- Which tests are deterministic, and which live checks remain probabilistic?
