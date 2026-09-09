# Task 1: Understand the Assistant and Check the Setup

**Timebox:** 8 minutes  
**Clock:** 0:00-0:08  
**Code changes:** None

**Prerequisite:** Complete
[Guide 00: Understand RAG and MCP](00-understand-rag-and-mcp.md).

## Outcome

Establish a trustworthy starting point. You should understand how conversation,
retrieval, grounding, weather logic, and MCP fit together before editing a TODO.

## Step 1: Confirm the Workspace

Open `starter/` as the VS Code workspace root. Confirm these paths exist:

```text
.env
.env.openweather
.github/
.vscode/
rag-data/
StormWatch.Chat/
StormWatch.Chat.Tests/
StormWatch.Tests/
stormwatch/
tests/
```

Do not open the populated environment files. Confirm that `git status` does not
list them. `.env` contains the chat and embedding deployment configuration;
`.env.openweather` isolates the weather credential. The `rag-data` folder
contains approved local knowledge files, never credentials or restricted data.

## Step 2: Build the Architecture Map

Read `README.md`, `StormWatch.Chat/Program.cs`, the records in
`stormwatch/Models.cs`, and each file containing a TODO. You should be able to
explain this flow:

```text
user conversation
      |
      v
StormWatch.Chat ---- embed ----> Foundry embedding model
      |                              |
      |<--- vectors -----------------+
      |
      +---- cosine search --------> local in-memory index over rag-data
      |
      +---- model request --------> Microsoft Foundry model
      |
      +---- MCP over stdio -------> StormWatch weather server
                                         |
                                         +--> OpenWeather
```

Retrieval supplies evidence. The model writes the answer. MCP supplies live or
fixture weather capabilities. None of those responsibilities should absorb the
others.

## Step 3: Verify the Toolchain

Run:

```powershell
dotnet --version
dotnet restore .\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj
dotnet restore .\StormWatch.Tests\StormWatch.Tests.csproj
```

The active SDK must be .NET 10. Package restore must finish before test failures
are interpreted.

## Step 4: Establish the Red Baseline

Run both suites:

```powershell
dotnet test .\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```

The starter discovers 22 tests. Failures should trace only to these checkpoints:

| Task | Files | Responsibility |
| --- | --- | --- |
| 2 | `StormWatch.Chat/ChatSession.cs` | Console loop and multi-turn history |
| 3 | `StormWatch.Chat/Knowledge.cs` | Local chunking, vector indexing, and cosine retrieval |
| 4 | `StormWatch.Chat/Grounding.cs`, `ChatSession.cs` | Safe context augmentation and citations |
| 5 | `stormwatch/Weather.cs`, `Risk.cs` | OpenWeather records and deterministic risk |
| 6 | `stormwatch/StormWatchTools.cs`, `StormWatch.Chat/McpToolSession.cs` | MCP exposure and chatbot connection |

Missing packages, undiscovered tests, compiler errors, or credential prompts in
unit tests are setup defects, not expected failures.

## Completion Gate

- [ ] .NET 10 is active and both projects restore.
- [ ] Exactly 22 tests are discovered across the two suites.
- [ ] Every failing test maps to one of Tasks 2-5; no unit test calls Foundry or
      OpenWeather.
- [ ] You can explain why local retrieval, answer generation, weather logic,
      and MCP transport are separate boundaries.
- [ ] You can identify which process receives each of the two private
      environment files and when `rag-data` text is sent for embedding.
- [ ] No source or test file was modified.

**Evidence to retain:** restore output, both red summaries, and your architecture
map.

## Sources

- [.NET CLI overview](https://learn.microsoft.com/dotnet/core/tools/) -
  available SDK commands and how the CLI selects an installed SDK.
- [dotnet restore](https://learn.microsoft.com/dotnet/core/tools/dotnet-restore) -
  restoring project dependencies and tools.
- [dotnet test](https://learn.microsoft.com/dotnet/core/tools/dotnet-test) -
  test discovery and execution with VSTest or Microsoft.Testing.Platform.
- [Configuration providers in .NET](https://learn.microsoft.com/dotnet/core/extensions/configuration-providers#environment-variable-configuration-provider) -
  loading configuration from environment variables.

Continue to [Task 2: Build a conversational chatbot](02-build-conversational-chatbot.md).
