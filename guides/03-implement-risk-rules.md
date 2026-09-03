# Task 3: Implement Storm-Risk Rules

**Timebox:** 15 minutes

**Clock:** 0:20-0:35
**Primary file:** `stormwatch/Risk.cs`

## Outcome

Implement a pure, deterministic scoring service. Every awarded indicator must
be explainable, threshold tiers must not double-count, the score must be capped,
and peak selection must have a stable tie-breaker.

This is an educational heuristic, not weather science.

## Step 1: Treat the Thresholds as Requirements

Open:

- the scoring table in `README.md`;
- `stormwatch/Models.cs`;
- `stormwatch/Risk.cs`;
- `StormWatch.Tests/RiskTests.cs`; and
- `StormWatch.Tests/TestData.cs`.

Translate the requirements into a decision table before coding:

| Indicator | Condition | Points |
| --- | --- | ---: |
| Thunderstorm | weather code 200 through 232, inclusive | 60 |
| Strong wind | at least 15 m/s | 25 |
| Elevated wind | at least 10 but less than 15 m/s | 15 |
| Heavy rain | at least 10 mm/3h | 20 |
| Elevated rain | at least 5 but less than 10 mm/3h | 10 |
| Very low pressure | at most 990 hPa | 15 |
| Low pressure | above 990 and at most 1000 hPa | 8 |

The two tiers within wind, rain, and pressure are mutually exclusive.

## Step 2: Write a Risk-Boundary Prompt

Write your own Copilot prompt for `Risk.cs`. It should point to the decision
table and focused tests, constrain the change to the risk boundary, preserve
explainability and determinism, and specify how the result will be verified.
Do not tell Copilot to change tests or move scoring into another layer.

## Step 3: Review `ScorePoint`

The scoring function should:

1. start at zero with an empty reason collection;
2. add 60 for a weather code in the inclusive 200-232 range;
3. use `if`/`else if` for wind tiers;
4. use `if`/`else if` for rain tiers;
5. use `if`/`else if` for pressure tiers;
6. add one human-readable reason for each awarded indicator; and
7. cap the final score at 100 without dropping reasons.

For the fixture peak, the raw indicators total 120:

```text
60 thunderstorm + 25 wind + 20 rain + 15 pressure = 120
```

The returned score is 100, while all four reasons remain visible.

## Step 4: Review `Assess`

The assessment function should:

- reject an empty forecast explicitly;
- score every forecast point;
- select the highest score;
- select the earliest timestamp if multiple points share that score;
- map 0-29 to `low`, 30-59 to `watch`, and 60-100 to `warning`; and
- return the peak point and its evidence in `StormAssessment`.

Do not use current time, random values, input order, or external services as a
tie-breaker.

## Step 5: Inspect the Diff

Run:

```powershell
git diff -- .\stormwatch\Risk.cs
git diff --name-only
```

Look for:

- independent `if` statements that award both lower and higher tiers;
- exclusive bounds accidentally written as `>` or `<`;
- level boundaries at 29/30 and 59/60;
- reasons removed when the score is capped;
- input collections being reordered or mutated; and
- scoring logic copied into CLI or MCP code.

## Step 6: Run Focused Verification

Run:

```powershell
dotnet test .\StormWatch.Tests\StormWatch.Tests.csproj --filter FullyQualifiedName~RiskTests
```

All eight risk tests must pass.

If a boundary test fails, compare the exact boundary against the decision table
instead of changing several conditions at once.

## If You Are Stuck

1. Make `ScorePoint` correct for one point before implementing `Assess`.
2. Use `else if` for tiers that represent alternatives.
3. Keep raw evidence collection separate from `Math.Min(score, 100)`.
4. Order candidates by score descending and timestamp ascending.
5. Use a score switch for the three level ranges.

## Completion Gate

You can say **“Task 3 is done”** only when all of the following are true:

- [ ] All eight `RiskTests` pass.
- [ ] Codes 200 and 232 score as thunderstorm conditions, while 199 and 233 do
      not.
- [ ] Wind, rain, and pressure award exactly one tier each at every boundary.
- [ ] A combined 120-point condition returns a capped score of 100 and retains
      four distinct evidence reasons.
- [ ] Scores 29/30 and 59/60 map to the correct adjacent levels.
- [ ] Equal peak scores choose the earliest timestamp regardless of input order.
- [ ] Empty forecasts fail explicitly instead of producing a fabricated low-risk
      result.
- [ ] `StormRiskService` is pure: it has no network, environment, clock, console,
      or mutation dependency.
- [ ] You can explain why deterministic tie-breaking and evidence are necessary
      for a result that will later be exposed through an MCP tool.

**Evidence to retain:** the eight-test pass summary, the threshold decision
table, and a reviewed diff showing mutually exclusive tiers.

Continue to [Task 4: Complete the CLI](04-complete-cli.md).
