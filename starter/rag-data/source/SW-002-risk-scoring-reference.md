# StormWatch Risk Scoring and Interpretation

Document ID: SW-002
Category: risk-policy
Version: 1.0
Status: Synthetic workshop reference
Security classification: workshop-public

## Scope

StormWatch calculates an educational signal from each typed forecast point. The
calculation is pure and deterministic: the same point always produces the same
score and reasons. It does not estimate storm probability, replace a forecast,
or read official watches and warnings.

## Scoring Table

| Indicator | Condition | Points |
| --- | --- | ---: |
| Thunderstorm condition | OpenWeather weather ID from 200 through 232, inclusive | 60 |
| High wind | Wind speed at least 15 m/s | 25 |
| Elevated wind | Otherwise, wind speed at least 10 m/s | 15 |
| Heavy three-hour rain | Rain at least 10 mm/3h | 20 |
| Elevated three-hour rain | Otherwise, rain at least 5 mm/3h | 10 |
| Very low pressure | Pressure at most 990 hPa | 15 |
| Low pressure | Otherwise, pressure at most 1000 hPa | 8 |

The wind, rain, and pressure tiers are mutually exclusive within each
indicator. A point with 16 m/s wind receives 25 wind points, not 25 plus 15. A
point with 12 mm/3h rain receives 20 rain points, not 20 plus 10. A pressure of
988 hPa receives 15 pressure points, not 15 plus 8.

Thunderstorm points are independent of the wind, rain, and pressure indicators.
Their points may be combined before the score cap is applied.

## Boundaries

Threshold comparisons are inclusive:

- Weather ID 200 and weather ID 232 both receive 60 points.
- Wind at exactly 15.0 m/s receives 25 points.
- Wind at exactly 10.0 m/s receives 15 points when it is below 15.0 m/s.
- Rain at exactly 10.0 mm/3h receives 20 points.
- Rain at exactly 5.0 mm/3h receives 10 points when it is below 10.0 mm/3h.
- Pressure at exactly 990 hPa receives 15 points.
- Pressure at exactly 1000 hPa receives 8 points when it is above 990 hPa.

Values just outside the boundary receive no points for that tier. For example,
9.9 m/s wind, 4.9 mm/3h rain, and 1001 hPa pressure each receive zero points.

## Score Cap and Levels

The raw indicator total is capped at 100 before it is displayed or assigned a
level.

| Displayed score | Level |
| ---: | --- |
| 0 through 29 | low |
| 30 through 59 | watch |
| 60 through 100 | warning |

These labels are StormWatch application labels. A StormWatch `warning` is not
an official meteorological warning.

## Assessment Across Forecast Points

StormWatch scores every forecast point and selects the point with the highest
displayed score as the peak. If two or more points have equal scores, the
earliest timestamp wins. This makes the output deterministic regardless of
collection sorting behavior.

An empty forecast is invalid and must be rejected. StormWatch cannot create an
assessment without at least one forecast point.

## Evidence Reasons

Every awarded indicator produces a human-readable reason. Reasons should name
the observed value and the relevant condition. A zero-point indicator does not
need a reason.

Example evidence for a high-risk point:

- Thunderstorm weather code 211 is within 200-232.
- Wind 16.4 m/s meets the 15 m/s high-wind threshold.
- Rain 12.0 mm/3h meets the 10 mm/3h heavy-rain threshold.
- Pressure 988 hPa meets the at-most-990 hPa very-low-pressure threshold.

Evidence is part of the assessment contract. A chatbot should not return only a
level or score when the tool supplies reasons.

## Worked Example A: Capped Warning

Input:

| Field | Value |
| --- | ---: |
| Weather ID | 211 |
| Wind | 16.4 m/s |
| Rain | 12.0 mm/3h |
| Pressure | 988 hPa |

Calculation:

- Thunderstorm: 60
- Wind: 25
- Rain: 20
- Pressure: 15
- Raw total: 120
- Displayed total after cap: 100
- Level: warning

The lower wind, rain, and pressure tiers are not added.

## Worked Example B: Combined Watch

Input:

| Field | Value |
| --- | ---: |
| Weather ID | 501 |
| Wind | 12.0 m/s |
| Rain | 6.4 mm/3h |
| Pressure | 998 hPa |

Calculation:

- Thunderstorm: 0 because 501 is outside 200-232
- Wind: 15
- Rain: 10
- Pressure: 8
- Total: 33
- Level: watch

## Worked Example C: Boundary-Safe Low

Input:

| Field | Value |
| --- | ---: |
| Weather ID | 500 |
| Wind | 9.9 m/s |
| Rain | 4.9 mm/3h |
| Pressure | 1001 hPa |

Every value is outside its lowest scoring threshold. The total is 0 and the
level is low. Rain can still be present even when the StormWatch score is zero.

## Interpretation Rules

- Describe the score as a rule-based educational heuristic.
- State the level, score, peak time, and every evidence reason.
- Preserve metric units and UTC timestamps.
- Do not convert a `warning` level into an official alert claim.
- Do not claim a probability such as "80 percent chance of a storm"; the score
  is not a calibrated probability.
- Do not infer travel safety, evacuation need, medical risk, or infrastructure
  readiness from the score.
- Direct users to official local weather and emergency services for operational
  decisions.

## Known Limitations

The heuristic uses only weather condition code, wind, three-hour rain, and
pressure. It does not consider lightning distance, hail size, radar, flood
models, terrain, drainage, building condition, storm motion, tropical cyclone
tracks, official alert polygons, observed conditions, forecast confidence, or
local emergency instructions.

Its largest limitation is that a simple weighted threshold score cannot capture
the uncertainty, geography, and hazard-specific reasoning of professional
meteorology.
