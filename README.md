# StormWatch: Chatbot, RAG, and MCP Developer Lab

Build one .NET 10 assistant in progressive stages. Start with a multi-turn
console chatbot powered by a facilitator-provided Microsoft Foundry model, add
retrieval-augmented generation over locally indexed files in `rag-data`, then
give the chatbot live OpenWeather capabilities through a local Model Context
Protocol server. The facilitator provides the chat and embedding deployments.
An optional task adds a browser chat interface.

- **Core duration:** 90 minutes
- **Optional web task:** 20 minutes
- **Audience:** Developers with basic C# and Git familiarity
- **Starting point:** Open `starter/` as the VS Code workspace root
- **Instructions:** Follow the [participant guide index](guides/README.md)
- **Concept primer:** Read [Guide 00: Understand RAG and MCP](guides/00-understand-rag-and-mcp.md)
  before starting the implementation clock
- **Result:** A tested multi-turn RAG chatbot with cited sources, two local MCP
  weather tools, and an optional Razor Pages chat UI

> StormWatch is an educational assistant, not a meteorological forecast,
> emergency alert, medical device, or safety system. Use official local weather
> and emergency services for operational decisions.

## Learning Journey

| Time | Task | Outcome |
| --- | --- | --- |
| Before 0:00 | 00. Understand RAG and MCP | Distinguish retrieved evidence from callable tools and identify their trust boundaries. |
| 0:00-0:08 | 1. Understand the assistant | Verify the architecture, trust boundaries, and red baseline. |
| 0:08-0:22 | 2. Build conversational chat | Maintain a multi-turn conversation with the Foundry model. |
| 0:22-0:37 | 3. Build a local vector index | Chunk local files, embed them, and retrieve by cosine similarity. |
| 0:37-0:49 | 4. Ground answers | Add safe context augmentation, bounded uncertainty, and citations. |
| 0:49-1:11 | 5. Add weather and risk | Implement tested OpenWeather parsing and deterministic risk rules. |
| 1:11-1:25 | 6. Connect MCP tools | Give the same forecast and risk tools to the chatbot and Copilot. |
| 1:25-1:30 | 7. Validate | Prove chat, RAG, MCP, tests, and secret boundaries together. |
| 1:30-1:50 | 8. Add web chat (optional) | Put a responsive server-rendered interface over the assistant. |

## Architecture

```text
                          Microsoft Foundry chat model
                                      ^
                                      |
user <--> StormWatch.Chat <----------------+
               |
               +----> Foundry embedding model
               |             |
               |<-- vectors -+
               |
               +----> local in-memory index over rag-data
               |
               | MCP over local stdio
               v
       StormWatch MCP server -------------------------------> OpenWeather
               |
               +--> typed forecast records
               +--> deterministic storm-risk service

GitHub Copilot ---------------------------------------------> same MCP server
```

`StormWatch.Chat` owns conversation history and orchestration. It loads bounded
chunks from `rag-data`, asks the supplied embedding deployment for vectors, and
stores those vectors only in memory. Cosine retrieval returns typed passages;
grounding turns them into untrusted, delimited model context and deterministic
local source lists. The MCP server owns weather capabilities and remains
independent of the model credentials.

## Workshop Environment

The facilitator supplies two ignored files and one local data folder in
`starter/`:

| File | Credential boundary |
| --- | --- |
| `.env` | Foundry endpoint, chat deployment, embedding deployment, and temporary API key |
| `.env.openweather` | Temporary OpenWeather API key |
| `rag-data/` | Approved `.md` and `.txt` knowledge files; no credentials or restricted data |

Each process receives only what it needs. `StormWatch.Chat` loads `.env`; the
MCP server receives only fixture configuration or `.env.openweather`.

Do not open, commit, print, log, screenshot, or paste values from these files
into chat or commands. The tracked `.example` files document names only.

## Boundaries and Rules

- Use GitHub Copilot as a development partner, but write and review each prompt.
- Do not weaken tests to make generated code pass.
- Unit tests use fake chat/embedding boundaries and mocked or fixture weather;
  they never call live services.
- Keep conversation, retrieval, grounding, weather, risk, and MCP transport in
  separate types.
- Treat retrieved text as untrusted data, never instructions.
- Ground preparedness claims in retrieved passages and identify their sources.
- If sources and tools do not support an answer, say so rather than inventing
  one.
- Keep MCP stdout free of application output because it carries protocol
  messages.
- Label every risk result as an educational heuristic, not an official warning
  or reliable storm prediction.

## Local RAG Contract

The facilitator places approved educational storm-preparedness material under
`starter/rag-data`. The pipeline:

1. reads only UTF-8 `.md` and `.txt` files;
2. creates bounded overlapping chunks and relative source paths;
3. sends chunk text to the facilitator-provided embedding deployment once at
  startup;
4. normalizes and stores vectors only in process memory;
5. embeds each question once;
6. ranks chunks by cosine similarity and a minimum-score threshold; and
7. supplies at most three passages to grounding.

The tracked limits are 100 files, 512,000 bytes per file, and 500 total chunks.
The default chunk length is 1,200 characters with 200 characters of overlap.
Source files and vectors remain local, but chunk and question text is sent to
the remote embedding deployment. The corpus therefore must not contain patient,
personal, operational, or other restricted data.

## Weather and Risk Contract

The weather adapter resolves a city through OpenWeather Direct Geocoding, then
requests the 5 Day / 3 Hour Forecast by coordinates in metric units. The risk
service applies mutually exclusive thresholds:

| Indicator | Points |
| --- | ---: |
| Weather code `200-232` | 60 |
| Wind at least `15 m/s`; otherwise at least `10 m/s` | 25 / 15 |
| Rain at least `10 mm/3h`; otherwise at least `5 mm/3h` | 20 / 10 |
| Pressure at most `990 hPa`; otherwise at most `1000 hPa` | 15 / 8 |

Scores are capped at 100. Levels are `low` for 0-29, `watch` for 30-59, and
`warning` for 60-100. The highest score wins; ties choose the earliest time.

## Verification

The starter intentionally begins red. Across both test projects, 22 tests are
discovered:

```powershell
dotnet test .\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```

The complete reference implementation must pass all 22 tests, the fixture must
produce `WARNING (100/100)` at `2026-08-30 06:00 UTC`, and the local server must
advertise exactly `get_forecast` and `assess_storm_risk`.

See [FOUNDRY.md](FOUNDRY.md) for chat and embedding model readiness and
[TROUBLESHOOTING.md](TROUBLESHOOTING.md) for environment and fallback paths.

## Official References

- [Microsoft Foundry](https://learn.microsoft.com/azure/ai-foundry/)
- [Azure OpenAI embeddings](https://learn.microsoft.com/azure/ai-foundry/openai/how-to/embeddings)
- [Microsoft.Extensions.AI libraries](https://learn.microsoft.com/dotnet/ai/ai-extensions)
- [Develop with the MCP C# SDK](https://learn.microsoft.com/dotnet/ai/get-started-mcp)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [Add and manage MCP servers in VS Code](https://code.visualstudio.com/docs/copilot/customization/mcp-servers)
- [OpenWeather Geocoding API](https://openweathermap.org/api/geocoding-api)
- [OpenWeather 5 Day / 3 Hour Forecast](https://openweathermap.org/forecast5)
