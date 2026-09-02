# Task 2: Build the Weather Adapter

**Timebox:** 13 minutes

**Clock:** 0:07-0:20
**Primary file:** `stormwatch/Weather.cs`

## Outcome

Implement a testable OpenWeather adapter that geocodes a city, retrieves a
metric forecast by coordinates, and maps JSON into the existing immutable
records. Errors must be useful without exposing the API key or a
credential-bearing URL.

## Step 1: Select Focused Context

Open only the files needed for this boundary:

- the weather-adapter section and architecture in `README.md`;
- `stormwatch/Models.cs`;
- `stormwatch/Weather.cs`;
- `StormWatch.Tests/WeatherTests.cs`;
- `StormWatch.Tests/TestData.cs`; and
- `tests/fixtures/forecast.json`.

Before editing, identify the expected flow:

```text
city
  -> HTTPS direct geocoding request
  -> first Location result
  -> HTTPS forecast request using latitude and longitude
  -> metric JSON
  -> Forecast and ForecastPoint records
```

## Step 2: Ask Copilot for One Boundary

Use:

```text
Implement only stormwatch/Weather.cs so WeatherTests passes. Use the injected
HttpClient. Resolve the city through OpenWeather Direct Geocoding, then request
the forecast by latitude and longitude with metric units. Use System.Text.Json,
a finite cancellation timeout, useful errors, and invariant-culture numbers.
Never include the API key or credential-bearing URL in an exception. Do not
change tests or public records. Run only WeatherTests.
```

If Copilot proposes edits outside `Weather.cs`, reject or narrow the change
unless the facilitator confirms a real compile requirement.

## Step 3: Review the HTTP Design

Before accepting the code, verify:

1. The constructor keeps and uses the injected `HttpClient`.
2. The city is validated, trimmed, and encoded as a query value.
3. The first request uses the HTTPS direct-geocoding endpoint with `limit=1`.
4. The second request uses latitude and longitude from the selected location.
5. The forecast request includes `units=metric` and does not repeat `q=<city>`.
6. Latitude and longitude use `CultureInfo.InvariantCulture`.
7. A linked cancellation token applies a finite timeout without discarding
   caller cancellation.

Do not accept code that creates a new `HttpClient` per request inside
`OpenWeatherClient` or disables certificate validation.

## Step 4: Review Parsing and Error Behavior

`ParseLocation` should:

- require a non-empty JSON array;
- map the first result into `Location`;
- preserve optional state information; and
- throw an actionable `WeatherServiceException` for unknown locations or an
  unexpected payload.

`ParseForecast` should:

- require a forecast `list`;
- map timestamp, temperature, weather code, description, wind, rain, and
  pressure into `ForecastPoint`;
- convert the Unix timestamp to UTC;
- treat a missing `rain.3h` property as `0.0`, not as an error; and
- reject an empty or structurally invalid forecast with a domain-level error.

HTTP, timeout, and JSON failures should be translated into concise messages.
The error text must not contain the API key or full credential-bearing URL.

## Step 5: Inspect the Diff

Run:

```powershell
git diff -- .\stormwatch\Weather.cs
git diff --name-only
```

The change should stay inside `Weather.cs`. Check for:

- accidental test edits;
- hard-coded keys;
- broad exception catches that hide defects;
- request URLs included in exception text; and
- parsing logic duplicated outside `WeatherParser`.

## Step 6: Run Focused Verification

Run:

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj --filter FullyQualifiedName~WeatherTests
```

All six weather tests must pass. If one fails, use the test name to inspect only
the corresponding boundary before asking Copilot for another change.

## If You Are Stuck

Ask for one targeted hint:

1. Parse the first geocoding array element into `Location`.
2. Build encoded query pairs with `Uri.EscapeDataString`.
3. Use `CancellationTokenSource.CreateLinkedTokenSource` and `CancelAfter`.
4. Map `rain.3h` with `TryGetProperty`; default it to zero.
5. Convert infrastructure exceptions into messages that omit URL and key data.

## Completion Gate

You can say **“Task 2 is done”** only when all of the following are true:

- [ ] All six `WeatherTests` pass without calling a live API.
- [ ] The mocked interaction performs two requests in order: geocoding first,
      coordinate forecast second.
- [ ] The forecast request uses metric units and invariant-culture coordinates,
      so behavior is stable across developer locales.
- [ ] A missing `rain.3h` value becomes `0.0`, while an unknown location or
      malformed payload produces an actionable domain error.
- [ ] Cancellation remains connected to the caller and each request has a finite
      timeout.
- [ ] The API key does not appear in exception messages, source literals, or a
      full request URL exposed to the user.
- [ ] Only the weather boundary changed; public records and tests remain intact.
- [ ] You can explain how injected `HttpClient` plus a fake message handler lets
      the test prove request construction without using the network.

**Evidence to retain:** the six-test pass summary and a reviewed `Weather.cs`
diff showing two-stage request construction and sanitized errors.

Continue to [Task 3: Implement storm-risk rules](03-implement-risk-rules.md).
