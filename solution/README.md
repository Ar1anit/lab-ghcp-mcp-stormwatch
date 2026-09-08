# StormWatch Reference Solution

This folder is for facilitator recovery and post-lab comparison. Participants
work in `starter/`. The reference includes the complete chatbot/RAG project,
weather and risk services, and MCP integration. Its test projects reuse the
same 22-test contract as the starter.

From the lab root:

```powershell
dotnet test .\solution\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj
dotnet test .\solution\StormWatch.Tests\StormWatch.Tests.csproj
dotnet run --project .\solution\stormwatch -- `
  Bengaluru --fixture .\starter\tests\fixtures\forecast.json
dotnet run --project .\solution\StormWatch.Smoke -- `
  .\solution\stormwatch\StormWatch.csproj `
  .\starter\tests\fixtures\forecast.json
```

The first suite validates multi-turn chat, local file chunking, vector ranking,
grounding, and sources through fake chat/embedding boundaries. The second
validates weather and risk through mocks and a fixture. The smoke client starts
the reference MCP server over stdio, discovers exactly two tools, and invokes
both with the synthetic fixture.

Live chat and embedding validation runs from `starter/`, where the facilitator
places `rag-data` and the two ignored environment files. The complete reference
chatbot can be copied there task by task when recovery is required.

To recover participant source from the `starter/` folder:

```powershell
Copy-Item -Force ..\solution\StormWatch.Chat\*.cs .\StormWatch.Chat\
Copy-Item -Force ..\solution\stormwatch\*.cs .\stormwatch\
dotnet test .\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```
