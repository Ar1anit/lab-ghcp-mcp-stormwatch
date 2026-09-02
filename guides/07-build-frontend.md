# Task 7: Build a Web Frontend

**Timebox:** 20 minutes

**Clock:** 1:00-1:20
**Primary project:** `StormWatch.Web/`

## Outcome

Create a responsive, server-rendered Razor Pages frontend that accepts a city,
loads the forecast through the existing application services, and presents the
five-period forecast and storm-risk assessment in a browser.

The browser must never receive the OpenWeather API key or call OpenWeather
directly. This task adds a presentation adapter; it does not create a second
weather or scoring implementation.

## Step 1: Define the Frontend Boundary

Keep this flow in mind:

```text
browser form
  -> Razor Page handler
  -> StormWatchDataService
  -> existing weather parser/client
  -> existing StormRiskService
  -> typed page model
  -> encoded HTML response
```

The frontend may choose layout, color, and responsive behavior. It must not:

- copy OpenWeather request code into JavaScript or the web project;
- copy thresholds from `Risk.cs`;
- expose `OPENWEATHER_API_KEY` through HTML, JavaScript, URLs, or JSON;
- parse the preformatted CLI report; or
- run inside the MCP stdio process.

## Step 2: Scaffold the Web Project

From the `starter/` directory, run:

```powershell
dotnet new webapp -n StormWatch.Web -f net10.0
dotnet add .\StormWatch.Web\StormWatch.Web.csproj reference .\stormwatch\StormWatch.csproj
dotnet build .\StormWatch.Web\StormWatch.Web.csproj
```

The project reference is the important boundary. It lets the page use the
existing records and services rather than recreating them.

Keep the generated Razor Pages structure. The expected working files are:

- `StormWatch.Web/Program.cs`;
- `StormWatch.Web/Pages/Index.cshtml`;
- `StormWatch.Web/Pages/Index.cshtml.cs`; and
- `StormWatch.Web/wwwroot/css/site.css`.

Do not add a frontend framework or third-party component library for this task.

## Step 3: Give Copilot Focused Context

Open:

- `stormwatch/Models.cs`;
- `stormwatch/Risk.cs`;
- `stormwatch/StormWatchTools.cs`;
- `StormWatch.Web/Program.cs`;
- `StormWatch.Web/Pages/Index.cshtml`;
- `StormWatch.Web/Pages/Index.cshtml.cs`; and
- `StormWatch.Web/wwwroot/css/site.css`.

Use this prompt:

```text
Create a server-rendered Razor Pages frontend in StormWatch.Web. Keep the
existing project reference to stormwatch. The page must accept a city, call the
injected StormWatchDataService with HttpContext.RequestAborted, assess the typed
Forecast with StormRiskService, and render the resolved location, data source,
five forecast periods, risk level, score, peak UTC time, evidence, and the full
educational disclaimer. Validate blank input and show concise user-safe errors.
Use semantic accessible HTML and responsive CSS. Do not copy weather, parsing,
CLI rendering, or scoring logic. Do not expose credentials or call OpenWeather
from browser code. Build the web project and run the existing tests.
```

Reject edits to the existing tests, thresholds, MCP tools, or weather transport
unless Copilot identifies a concrete compile problem and you verify it.

## Step 4: Review Server-Side Composition

Before running the page, confirm:

1. `Program.cs` registers Razor Pages and `StormWatchDataService`.
2. The page model receives `StormWatchDataService` through dependency injection.
3. The POST handler trims and validates the city before loading data.
4. `LoadAsync` receives `HttpContext.RequestAborted`.
5. `StormRiskService.Assess` evaluates the returned typed `Forecast`.
6. Only expected argument, weather-service, and I/O failures become a visible
   page error; cancellation is not turned into a successful response.
7. The handler does not read or return `OPENWEATHER_API_KEY`.
8. The generated Razor Pages antiforgery protection remains enabled for the
   POST form.

Use the domain records directly or map them into a small display model. Do not
call `StormWatchApp.RenderReport` and then parse its text.

The web host is a separate process from the MCP stdio server. Normal ASP.NET
Core logging must never be added to the MCP process's stdout path.

## Step 5: Review the Page and Styling

The page should provide:

- a visible heading and short explanation of the educational purpose;
- a labeled city input with preserved input after submission;
- a clear submit button and validation summary;
- resolved location and whether data came from OpenWeather or the fixture;
- a prominent risk level and score;
- peak time in UTC;
- every evidence reason;
- exactly five forecast periods with time, temperature, description, wind,
  rain, and pressure units; and
- the full non-official-warning disclaimer next to the result.

Use semantic headings, labels, lists, and a table or accessible card structure.
Ensure keyboard focus is visible and text contrast remains readable. At a
roughly 375-pixel viewport, content must reflow without horizontal page
scrolling.

Razor should encode weather descriptions and city values by default. Do not use
`Html.Raw` for data returned by OpenWeather.

## Step 6: Run the Deterministic Acceptance Path

Use the fixture so the result is stable and no key is needed:

```powershell
$env:STORMWATCH_FIXTURE_PATH = (Resolve-Path .\tests\fixtures\forecast.json).Path
dotnet run --project .\StormWatch.Web\StormWatch.Web.csproj -- --urls http://localhost:5178
```

Open <http://localhost:5178>, enter `Bengaluru`, and submit.

Confirm the page shows:

- `Bengaluru, Karnataka, IN`;
- source `synthetic fixture`;
- exactly five forecast periods;
- `WARNING` and `100/100`;
- peak `2026-08-30 06:00 UTC`;
- all four evidence reasons; and
- the educational disclaimer.

Keep the server bound to `localhost` for the lab. After stopping it, clear the
fixture variable if you will switch to live mode:

```powershell
Remove-Item Env:STORMWATCH_FIXTURE_PATH -ErrorAction SilentlyContinue
```

For live mode, set `OPENWEATHER_API_KEY` only in the server process environment.
Never place it in Razor, JavaScript, `appsettings.json`, launch URLs, or source.

## Step 7: Exercise Negative and Accessibility Paths

Perform these checks:

1. Submit a blank or whitespace-only city. The page should show validation and
   must not attempt a forecast load.
2. Use keyboard-only navigation. The label, input, button, error, result, and
   evidence must be understandable in focus order.
3. Test desktop and narrow mobile widths. The result must remain readable
   without clipped controls or horizontal page scrolling.
4. Inspect browser developer tools. Browser requests should stay on localhost;
   there must be no request to OpenWeather and no `appid=` query parameter.
5. View the rendered page source and network responses. They must contain no
   credential, stack trace, or credential-bearing URL.

## Step 8: Run the Final Checks

Stop the web server, then run:

```powershell
dotnet build .\StormWatch.Web\StormWatch.Web.csproj
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj
git -c core.whitespace=cr-at-eol diff --check
git diff --name-only
git diff -- .\StormWatch.Web
```

The original 17 tests must still pass. Review every change outside
`StormWatch.Web/`; the frontend should not require weaker tests or altered
domain rules.

## If You Are Stuck

1. Confirm the web project references `stormwatch/StormWatch.csproj`.
2. Register `StormWatchDataService` as a singleton in the web host.
3. Keep `Forecast`, `StormAssessment`, and `Source` as page-model properties.
4. Handle the form in `OnPostAsync` and pass `HttpContext.RequestAborted`.
5. Use the fixture environment variable before investigating live API behavior.
6. If port 5178 is occupied, choose another unused localhost port.

## Completion Gate

You can say **"Task 7 is done"** only when all of the following are true:

- [ ] The web project builds and the original 17 tests still pass unchanged.
- [ ] Submitting `Bengaluru` in fixture mode renders the resolved location,
      fixture source, five periods, `WARNING (100/100)`, the expected peak, all
      evidence, units, and the complete disclaimer.
- [ ] Blank input is rejected before any data load, while expected service
      failures produce a concise page error without a stack trace.
- [ ] The page composes `StormWatchDataService` and `StormRiskService`; it
      contains no copied HTTP, JSON parsing, CLI formatting, or scoring logic.
- [ ] Cancellation flows from `HttpContext.RequestAborted` into the existing
      data service.
- [ ] The browser makes no OpenWeather request and receives no API key,
      `appid=` query, credential-bearing URL, or server exception detail.
- [ ] External city and weather text is Razor-encoded rather than rendered with
      unsafe raw HTML.
- [ ] The POST form retains Razor Pages antiforgery protection.
- [ ] The form and results are keyboard-usable, labeled, readable at desktop and
      mobile widths, and free from horizontal page scrolling.
- [ ] The web host remains separate from the MCP stdio process, so frontend
      logging cannot corrupt MCP protocol traffic.
- [ ] You can explain why the browser/server boundary keeps the credential
      server-side and why the frontend is a presentation adapter rather than a
      second application core.

**Evidence to retain:** the successful web build and 17-test summary, one
desktop and one narrow-width screenshot, the deterministic Bengaluru result,
and a developer-tools view showing only localhost browser requests.

Return to the [participant guide index](README.md).
