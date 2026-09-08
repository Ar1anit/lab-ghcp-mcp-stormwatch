# StormWatch Reference Guide

This reference mirrors the redesigned participant route and points facilitators
to the complete implementation under `solution/`.

- **Required route:** 90 minutes
- **Optional web-chat task:** 20 minutes
- **Participant workspace:** `starter/`
- **Reference projects:** `solution/StormWatch.Chat`, `solution/stormwatch`
- **Automated contract:** 5 chatbot/RAG tests plus 17 weather/application tests

## Participant Route

1. [Understand the assistant and check the setup](../guides/01-understand-app-and-check-setup.md) - 8 minutes
2. [Build a conversational chatbot](../guides/02-build-conversational-chatbot.md) - 14 minutes
3. [Build a local vector index](../guides/03-build-local-vector-index.md) - 15 minutes
4. [Ground answers with citations](../guides/04-ground-answers-with-citations.md) - 12 minutes
5. [Add weather and risk services](../guides/05-add-weather-and-risk-services.md) - 22 minutes
6. [Connect OpenWeather MCP tools](../guides/06-connect-openweather-mcp-tools.md) - 14 minutes
7. [Test the complete chatbot](../guides/07-test-complete-chatbot.md) - 5 minutes
8. [Add a web chat interface](../guides/08-add-web-chat-interface.md) - 20 minutes, optional

See the [participant index](../guides/README.md) for completion gates and
[FACILITATOR.md](FACILITATOR.md) for delivery and recovery guidance.

## Reference Architecture

```text
user
  |
  v
console loop -> GroundedChatSession -> IChatCompletionService -> Foundry model
                         |
                         +-> IKnowledgeRetriever -> local vector index
                                                      |
                                                      +-> rag-data
                         |
                         +-> AITool list -> local MCP client
                                                |
                                                v
                                      StormWatch MCP server
                                         |             |
                                         v             v
                                   OpenWeather     risk service
```

The reference implementation demonstrates these boundaries:

- `ChatSession.cs` owns successful-turn history and orchestration.
- `Knowledge.cs` owns local file loading, embeddings, cosine ranking, and
  passage mapping.
- `Grounding.cs` owns context delimiters and deterministic source lists.
- `McpToolSession.cs` owns local server startup and exact tool discovery.
- `Weather.cs` owns OpenWeather transport and parsing.
- `Risk.cs` owns deterministic scoring.
- `StormWatchTools.cs` maps shared services into structured MCP results.

## Starter Checkpoints

| Task | TODO location | Reference behavior |
| --- | --- | --- |
| 2 | `StormWatch.Chat/ChatSession.cs` | Multi-turn session plus console loop |
| 3 | `StormWatch.Chat/Knowledge.cs` | Local chunk embeddings and top-three cosine retrieval |
| 4 | `StormWatch.Chat/Grounding.cs`, `ChatSession.cs` | Untrusted context, citations, natural history |
| 5 | `stormwatch/Weather.cs`, `Risk.cs` | Safe forecast adapter and explained risk rules |
| 6 | `stormwatch/StormWatchTools.cs`, `StormWatch.Chat/McpToolSession.cs` | Structured tools and chatbot MCP connection |

`StormWatchApp.cs` remains complete shared loading and fixture infrastructure;
it is no longer a participant implementation checkpoint.

## Reference Validation

```powershell
dotnet test .\solution\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj
dotnet test .\solution\StormWatch.Tests\StormWatch.Tests.csproj
dotnet run --project .\solution\StormWatch.Smoke -- `
  .\solution\stormwatch\StormWatch.csproj `
  .\starter\tests\fixtures\forecast.json
```

The tests prove local behavior through fake chat/embedding boundaries and mocked
or fixture weather. They do not prove live service availability, corpus quality,
retrieval relevance, citation truth, or production authorization.

## Facilitator-Supplied Corpus and Embedding Model

The facilitator supplies approved `.md` and `.txt` files in `rag-data` plus an
embedding deployment name in `.env`. Participants build the in-memory vector
index and grounding path. They do not provision a search service or persist
vectors.

Use approved educational material with recognizable filenames. Prepare a known-good
question and an unsupported question so the live gate evaluates both grounding
and bounded uncertainty.

## Security Invariants

- `.env` and `.env.openweather` remain ignored and separate.
- MCP receives no chat or embedding model credential.
- Retrieved passages are untrusted data and cannot override system behavior.
- Unit tests never call live services.
- MCP stdout carries protocol messages only.
- Browser code never calls Foundry or OpenWeather directly.
- Storm-risk output is educational, not operational guidance.

## Official References

- [Microsoft Foundry](https://learn.microsoft.com/azure/ai-foundry/)
- [Azure OpenAI embeddings](https://learn.microsoft.com/azure/ai-foundry/openai/how-to/embeddings)
- [Microsoft.Extensions.AI libraries](https://learn.microsoft.com/dotnet/ai/ai-extensions)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [MCP servers in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers)
