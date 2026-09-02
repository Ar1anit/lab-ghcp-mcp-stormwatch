# C# Reference Solution

This folder is for facilitator recovery and post-lab comparison. Participants
work in `starter/`; both projects use the same 17 xUnit contract sources.

From the lab root:

```powershell
dotnet test .\solution\StormWatch.Tests\StormWatch.Tests.csproj
dotnet run --project .\solution\StormWatch -- `
  Bengaluru --fixture .\starter\tests\fixtures\forecast.json
dotnet run --project .\solution\StormWatch.Smoke -- `
  .\solution\StormWatch\StormWatch.csproj `
  .\starter\tests\fixtures\forecast.json
```

The smoke client and server both use the official `ModelContextProtocol` C# SDK.
It starts the reference server over stdio, discovers exactly two tools, and
invokes both using the synthetic fixture.

To recover participant source from the `starter/` folder:

```powershell
Copy-Item -Force ..\solution\StormWatch\*.cs .\StormWatch\
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
```