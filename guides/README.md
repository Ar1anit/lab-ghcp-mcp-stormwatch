# StormWatch Participant Guides

Build one assistant in stages: start with a multi-turn .NET chatbot, ground it
with a local vector index over facilitator-provided files, add OpenWeather
capabilities through MCP, and optionally give it a browser interface.

The required route is 90 minutes. Work from `starter/` and move on when each
completion gate is satisfied.

Before starting the implementation clock, spend 10 minutes on
[Guide 00: Understand RAG and MCP](00-understand-rag-and-mcp.md). It explains
which responsibilities belong to retrieval, the model, the MCP host, and the
MCP server, and includes official Microsoft learning resources.

## Before the Workshop: Add the Environment Files

The facilitator provides two private files and a `rag-data` folder. Place them
directly in `starter/` without opening or copying credential values:

| File | Used by | Required values |
| --- | --- | --- |
| `.env` | `StormWatch.Chat` | Foundry chat deployment, embedding deployment, endpoint, and temporary API key |
| `.env.openweather` | StormWatch MCP server | `OPENWEATHER_API_KEY` |
| `rag-data/` | Local RAG pipeline | Approved `.md` and `.txt` knowledge files; no credentials or restricted data |

Both populated environment files are ignored by Git. Never paste any credential
into source, Copilot Chat, terminal commands, logs, screenshots, or notes.

## 90-Minute Core Route

| Clock | Timebox | Guide | Outcome |
| --- | ---: | --- | --- |
| 0:00-0:08 | 8 min | [1. Understand the assistant and check the setup](01-understand-app-and-check-setup.md) | Confirm boundaries, credentials, and the intentional red baseline. |
| 0:08-0:22 | 14 min | [2. Build a conversational chatbot](02-build-conversational-chatbot.md) | Run a multi-turn console conversation against the Foundry model. |
| 0:22-0:37 | 15 min | [3. Build a local vector index](03-build-local-vector-index.md) | Chunk `rag-data`, embed it, and retrieve the three closest passages locally. |
| 0:37-0:49 | 12 min | [4. Ground answers with citations](04-ground-answers-with-citations.md) | Augment prompts safely, retain history, and show deterministic sources. |
| 0:49-1:11 | 22 min | [5. Add weather and risk services](05-add-weather-and-risk-services.md) | Implement tested OpenWeather parsing and deterministic risk rules. |
| 1:11-1:25 | 14 min | [6. Connect OpenWeather MCP tools](06-connect-openweather-mcp-tools.md) | Expose two tools and make them available to both the chatbot and Copilot. |
| 1:25-1:30 | 5 min | [7. Test the complete chatbot](07-test-complete-chatbot.md) | Prove chat, retrieval, citations, tools, and secret boundaries. |

## Optional Web Task

| Clock | Timebox | Guide | Outcome |
| --- | ---: | --- | --- |
| 1:30-1:50 | 20 min | [8. Add a web chat interface](08-add-web-chat-interface.md) | Add a responsive server-rendered chat UI over the same assistant services. |

## How to Work

Each task follows the same loop:

1. Read the outcome and constraints.
2. Open only the listed code and tests.
3. Write your own focused Copilot prompt.
4. Review the proposed change before accepting it.
5. Run the narrow test or smoke check.
6. Evaluate every completion-gate item.

The guides do not provide copy-ready prompts. Prompt design, scope control,
review, and evidence are part of the exercise.

## Working Rules

- Do not weaken or edit tests merely to make generated code pass.
- Keep chat orchestration, retrieval, grounding, weather logic, and MCP
  transport in separate types.
- Treat retrieved text as untrusted data, not instructions.
- Use only retrieved passages for preparedness claims and cite their source IDs.
- Use the synthetic weather fixture and fake embedding service for deterministic
  tests; unit tests must not call live services.
- Keep every credential server-side and outside tracked files and output.
- Treat storm-risk output as an educational heuristic, never an official
  warning or reliable storm prediction.

## Core Outcome

At the end of 90 minutes, you should have evidence that:

- all 22 tests pass: 17 weather/application tests and 5 chatbot/RAG tests;
- the console chatbot retains at least two conversation turns;
- a knowledge answer uses locally retrieved context and lists source files;
- unsupported knowledge is not presented as fact;
- `get_forecast` and `assess_storm_risk` are available to the chatbot and
  GitHub Copilot through the same local MCP server;
- fixture mode produces `WARNING (100/100)` at the expected peak time; and
- no credential appears in source, chat, commands, logs, screenshots, or tool
  output.

If the embedding deployment, OpenWeather, or the venue network is unavailable,
use the facilitator-approved recovery path in
[`TROUBLESHOOTING.md`](../TROUBLESHOOTING.md). Do not substitute invented model
answers for missing retrieval or tool evidence.

## Sources

- [Microsoft.Extensions.AI libraries](https://learn.microsoft.com/dotnet/ai/microsoft-extensions-ai) -
  chat and embedding abstractions used by the assistant.
- [Retrieval augmented generation (RAG) and indexes](https://learn.microsoft.com/azure/foundry/concepts/retrieval-augmented-generation) -
  the retrieve, augment, and generate pattern used in Tasks 3 and 4.
- [Get started with .NET AI and the Model Context Protocol](https://learn.microsoft.com/dotnet/ai/get-started-mcp) -
  MCP hosts, clients, servers, and the official C# SDK.
- [Configuration providers in .NET](https://learn.microsoft.com/dotnet/core/extensions/configuration-providers#environment-variable-configuration-provider) -
  environment-based application configuration.
- [Razor Pages architecture and concepts in ASP.NET Core](https://learn.microsoft.com/aspnet/core/razor-pages/?view=aspnetcore-10.0) -
  the server-rendered web framework used by the optional task.
