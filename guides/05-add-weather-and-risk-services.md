# Task 5: Add Weather and Risk Services

**Timebox:** 22 minutes  
**Clock:** 0:49-1:11  
**Primary files:** `stormwatch/Weather.cs`, `stormwatch/Risk.cs`

## Outcome

Implement the tested domain capabilities the chatbot will receive through MCP:
typed OpenWeather forecasts and deterministic, explainable storm-risk signals.

## Step 1: Select Focused Context

For weather, open `Models.cs`, `Weather.cs`, `WeatherTests.cs`, `TestData.cs`,
and `tests/fixtures/forecast.json`. For risk, open `Risk.cs` and `RiskTests.cs`.
Do not edit `StormWatchApp.cs`; it is complete shared loading infrastructure.

## Step 2: Implement OpenWeather Retrieval

`OpenWeatherClient.GetForecastAsync` must:

1. validate, trim, and encode the city;
2. call Direct Geocoding over HTTPS with `limit=1`;
3. call the 5 Day / 3 Hour Forecast endpoint using resolved coordinates;
4. request metric units and use invariant coordinate formatting;
5. preserve caller cancellation while applying a finite timeout; and
6. translate HTTP, timeout, and JSON failures into safe domain errors.

`ParseLocation` maps the first geocoding result. `ParseForecast` maps timestamp,
temperature, weather ID and description, wind, `rain.3h`, and pressure. Missing
rain becomes `0.0`. Errors must not contain the API key or a credential-bearing
URL.

Run the six focused tests:

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj `
  --filter FullyQualifiedName~WeatherTests
```

## Step 3: Implement Deterministic Risk

Use these mutually exclusive thresholds:

| Indicator | Points |
| --- | ---: |
| Weather code `200-232` | 60 |
| Wind at least `15 m/s` | 25 |
| Otherwise wind at least `10 m/s` | 15 |
| Rain at least `10 mm/3h` | 20 |
| Otherwise rain at least `5 mm/3h` | 10 |
| Pressure at most `990 hPa` | 15 |
| Otherwise pressure at most `1000 hPa` | 8 |

Every awarded indicator needs a reason. Cap the total at 100. Levels are
`low` for 0-29, `watch` for 30-59, and `warning` for 60-100. Select the
highest-scoring point; ties choose the earliest timestamp. Reject an empty
forecast.

Run the eight focused tests:

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj `
  --filter FullyQualifiedName~RiskTests
```

## Step 4: Run the Full Domain Suite

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
dotnet run --project .\stormwatch -- `
  Bengaluru --fixture .\tests\fixtures\forecast.json
```

All 17 tests must pass. The fixture reports `WARNING (100/100)` with peak
`2026-08-30 06:00 UTC`. Live weather is useful exploration but not acceptance
evidence because it changes over time.

## Completion Gate

- [ ] All 17 domain/application tests pass without a live request.
- [ ] Requests geocode first, then forecast by invariant coordinates in metric
      units.
- [ ] Unknown cities, malformed payloads, timeouts, and HTTP failures are safe
      and actionable.
- [ ] Tiered risk indicators do not double-count and every score is explained.
- [ ] Fixture mode produces the expected score and peak time without a key.
- [ ] Weather transport and risk scoring remain separate and deterministic.
- [ ] No credential appears in source, errors, output, or commands.

**Evidence to retain:** the 17-test pass summary and fixture report.

## Sources

- [Make HTTP requests with the HttpClient class](https://learn.microsoft.com/dotnet/fundamentals/networking/http/httpclient) -
  request construction, response handling, timeouts, and cancellation.
- [How to read JSON as .NET objects](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/deserialization) -
  typed and asynchronous JSON deserialization with `System.Text.Json`.

Continue to [Task 6: Connect OpenWeather MCP tools](06-connect-openweather-mcp-tools.md).
