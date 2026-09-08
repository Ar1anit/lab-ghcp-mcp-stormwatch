# StormWatch Weather Data and MCP Tool Contracts

Document ID: SW-003
Category: data-and-tools
Version: 1.0
Status: Synthetic workshop reference
Security classification: workshop-public

## Data Acquisition

Live mode resolves a user-supplied city with OpenWeather Direct Geocoding before
requesting the five-day, three-hour forecast by latitude and longitude. Requests
use HTTPS, metric units, cancellation, and a finite timeout.

Fixture mode reads the supplied synthetic JSON file and makes no live request.
It is the preferred workshop validation path because it is deterministic and
does not require an OpenWeather key.

The API key is read only from `OPENWEATHER_API_KEY` in live mode. It must not
appear in source, output, logs, exceptions, returned URLs, MCP responses, or
browser content.

## Domain Records

### Location

| Field | Type | Meaning |
| --- | --- | --- |
| `Name` | string | Resolved city name |
| `Country` | string | Country code |
| `Latitude` | number | Decimal latitude |
| `Longitude` | number | Decimal longitude |
| `State` | optional string | Resolved state or region |
| `DisplayName` | derived string | Non-empty name, state, and country joined for display |

### ForecastPoint

| Field | Unit | Meaning |
| --- | --- | --- |
| `Timestamp` | UTC date-time | Start time of the forecast period |
| `TemperatureC` | degrees Celsius | Forecast temperature |
| `WeatherId` | OpenWeather condition ID | Numeric weather classification |
| `Description` | text | Human-readable weather description |
| `WindSpeedMps` | meters per second | Forecast wind speed |
| `Rain3hMm` | millimeters per three hours | Rain accumulated in the period |
| `PressureHpa` | hectopascals | Forecast atmospheric pressure |

If the OpenWeather payload omits the three-hour rain field, StormWatch maps it
to `0.0` mm/3h. Missing rain does not mean missing forecast data; it means no
reported three-hour rain amount for that point.

### Forecast and StormAssessment

A `Forecast` combines one resolved `Location` with a non-empty ordered
collection of points. A `StormAssessment` contains a level, capped score, peak
forecast point, and evidence reasons.

## Display Units

StormWatch scoring and structured output use metric units:

- temperature: degrees Celsius;
- wind speed: meters per second (`m/s`);
- rain: millimeters per three hours (`mm/3h`);
- pressure: hectopascals (`hPa`); and
- time: UTC with an explicit offset or UTC label.

A future interface may display converted values, but scoring must remain metric
and the original units must be unambiguous.

## MCP Tool Inventory

The stdio server advertises exactly two tools.

### get_forecast

Purpose: Get the next five OpenWeather forecast points for a city.

Input:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `city` | string | yes | City name, optionally followed by a country code |

Structured result:

| Field | Meaning |
| --- | --- |
| `location` | Resolved display name |
| `coordinates` | Latitude and longitude |
| `source` | `OpenWeather 5 Day / 3 Hour Forecast` or `synthetic fixture` |
| `units` | Labels for temperature, wind, rain, and pressure |
| `periods` | Exactly the next five typed forecast periods |

Each period contains UTC time, temperature in Celsius, description, weather ID,
wind in m/s, rain in mm/3h, and pressure in hPa.

Use `get_forecast` when the user asks what the next periods contain but does not
need the educational score.

### assess_storm_risk

Purpose: Assess the educational storm-risk signal for a city's forecast.

Input:

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `city` | string | yes | City name, optionally followed by a country code |

Structured result:

| Field | Meaning |
| --- | --- |
| `location` | Resolved display name |
| `source` | Live OpenWeather or synthetic fixture |
| `level` | `low`, `watch`, or `warning` |
| `score` | Integer from 0 through 100 |
| `peakTimeUtc` | Selected peak timestamp |
| `evidence` | Every reason that contributed points |
| `disclaimer` | Educational-use and non-official-warning qualification |

Use `assess_storm_risk` when the user asks for risk, score, peak, evidence, or an
explanation of why a period was selected. The tool composes the existing
weather and risk services; it does not duplicate their logic.

## RAG and Tool Routing

| User need | Retrieval | MCP tool |
| --- | --- | --- |
| Explain the 990 hPa threshold | Risk scoring PDF | none |
| Show the next five periods for Oslo | Optional field definitions | `get_forecast` |
| Assess risk for Bengaluru | Responsible-use guidance | `assess_storm_risk` |
| Explain a returned score | Risk scoring PDF | `assess_storm_risk` |
| Describe MCP architecture | System overview PDF | none |
| Ask whether an official alert exists | Responsible-use PDF | none; direct user to official source |

The PDF corpus contains no current forecast. A city name in a scenario does not
authorize the chatbot to skip an MCP call.

## Error Contract

Expected failures include blank city input, unknown city, unsuccessful HTTP
response, timeout, cancellation, invalid JSON, missing fixture, unreadable
fixture, and missing live-mode configuration.

For a normal user-facing failure, the chatbot should:

1. say that StormWatch could not load or assess the requested data;
2. provide a concise, non-secret reason when available;
3. suggest correcting the city, checking configuration, or retrying later;
4. avoid a stack trace or credential-bearing URL; and
5. never manufacture forecast points or a score.

Cancellation should propagate rather than becoming a successful empty result.
Unit tests and workshop evaluation must not call live services.

## Concision and Determinism

Tool responses are structured rather than preformatted reports. They should
contain only the fields needed by the caller, use invariant numeric and
timestamp formats, and never write normal application output to stdio. These
properties let both GitHub Copilot and the local Foundry client inspect the same
contract reliably.
