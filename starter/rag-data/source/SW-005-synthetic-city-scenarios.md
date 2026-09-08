# StormWatch Synthetic City Scenarios

Document ID: SW-005
Category: training-scenarios
Version: 1.0
Status: Synthetic workshop reference
Security classification: workshop-public

## How to Use These Scenarios

Every value in this document is fictional and fixed for training. City names
make retrieval exercises realistic; they do not indicate historical, current,
or future conditions in those cities.

Use scenarios to test threshold explanations, score calculations, retrieval
citations, and source precedence. Never use a scenario to answer "right now,"
"today," or another current city question. Current or configured fixture facts
must come from the StormWatch MCP tools.

## Scenario A: Bengaluru Capped Warning

Training date: 2026-08-30

| Time UTC | Weather ID | Wind m/s | Rain mm/3h | Pressure hPa | Score |
| --- | ---: | ---: | ---: | ---: | ---: |
| 03:00 | 500 | 9.0 | 5.2 | 1002 | 10 |
| 06:00 | 211 | 16.4 | 12.0 | 988 | 100 capped |
| 09:00 | 202 | 10.4 | 4.0 | 995 | 83 |

Expected assessment: `warning`, 100/100, peak at 06:00 UTC.

The 06:00 raw total is 120: thunderstorm 60, high wind 25, heavy rain
20, and very low pressure 15. The displayed score is capped at 100. The
09:00 period is also a warning but does not beat the capped 06:00 score.

Retrieval trap: A current Bengaluru question must call MCP. This scenario is not
a current forecast.

## Scenario B: Rotterdam Combined Watch

Training date: 2026-10-12

| Time UTC | Weather ID | Wind m/s | Rain mm/3h | Pressure hPa | Score |
| --- | ---: | ---: | ---: | ---: | ---: |
| 12:00 | 501 | 12.0 | 6.4 | 998 | 33 |
| 15:00 | 500 | 11.0 | 2.0 | 1005 | 15 |
| 18:00 | 802 | 8.5 | 0.0 | 1008 | 0 |

Expected assessment: `watch`, 33/100, peak at 12:00 UTC.

At 12:00, elevated wind contributes 15, elevated rain contributes 10, and low
pressure contributes 8. Weather ID 501 is not a thunderstorm code.

## Scenario C: Seattle Boundary-Safe Low

Training date: 2026-11-03 at 09:00 UTC.

| Weather ID | Wind m/s | Rain mm/3h | Pressure hPa |
| ---: | ---: | ---: | ---: |
| 500 | 9.9 | 4.9 | 1001 |

Expected assessment: `low`, 0/100.

Each value is just outside the lowest scoring threshold. This scenario proves
that presence of rain alone does not guarantee points and that comparisons must
not be rounded before scoring.

## Scenario D: Miami Thunderstorm-Only Warning

Training date: 2027-01-15 at 18:00 UTC.

| Weather ID | Wind m/s | Rain mm/3h | Pressure hPa |
| ---: | ---: | ---: | ---: |
| 201 | 4.0 | 1.0 | 1011 |

Expected assessment: `warning`, 60/100.

Weather ID 201 is within 200-232 and contributes 60 points. The other
indicators add zero. A thunderstorm condition alone reaches the warning band,
but the result is still not an official warning.

## Scenario E: Reykjavik Exact Thresholds

Training date: 2027-02-02 at 00:00 UTC.

| Weather ID | Wind m/s | Rain mm/3h | Pressure hPa |
| ---: | ---: | ---: | ---: |
| 804 | 15.0 | 0.0 | 990 |

Expected assessment: `watch`, 40/100.

Wind at exactly 15.0 m/s contributes 25. Pressure at exactly 990 hPa
contributes 15. Both comparisons are inclusive.

## Scenario F: Mumbai Multi-Indicator Warning Without Thunder

Training date: 2027-03-21 at 15:00 UTC.

| Weather ID | Wind m/s | Rain mm/3h | Pressure hPa |
| ---: | ---: | ---: | ---: |
| 502 | 15.0 | 10.0 | 990 |

Expected assessment: `warning`, 60/100.

High wind, heavy three-hour rain, and very low pressure contribute 25, 20, and
15 points. Weather ID 502 is outside the thunderstorm range. This proves that
combined non-thunderstorm indicators can still reach warning.

## Scenario G: Hamburg Earliest Tie-Break

Training date: 2027-04-08

| Time UTC | Weather ID | Wind m/s | Rain mm/3h | Pressure hPa | Score |
| --- | ---: | ---: | ---: | ---: | ---: |
| 03:00 | 500 | 12.0 | 5.0 | 1000 | 33 |
| 06:00 | 501 | 10.0 | 7.0 | 999 | 33 |
| 09:00 | 800 | 5.0 | 0.0 | 1012 | 0 |

Expected assessment: `watch`, 33/100, peak at 03:00 UTC.

The first two periods tie. StormWatch selects the earliest timestamp, 03:00
UTC, even though both periods have valid but slightly different evidence.

## Scenario H: Source Conflict Exercise

Assume a learner asks, "What is Rotterdam's risk now?" Retrieval returns
Scenario B with a score of 33. A successful `assess_storm_risk` call returns a
different configured result with a score of 15.

Expected chatbot behavior:

1. Use the MCP result of 15 for the requested dynamic fact.
2. State that Scenario B is fixed training data, not current weather.
3. Use the scoring reference to explain the returned evidence if requested.
4. Include the educational-use qualification.

Incorrect behavior would present 33 as current, blend the two scores, or omit
the source distinction.

## Scenario Discussion Prompts

- Which single indicator makes Scenario D a warning?
- Why does Scenario C remain low despite nonzero rain?
- Which thresholds are exactly met in Scenario E?
- How can Scenario F reach warning without a thunderstorm code?
- Why does 03:00 win in Scenario G?
- Which source should answer a current Rotterdam question?

Every correct answer should cite this scenario document for fixed example facts
and the risk scoring reference for general threshold policy.
