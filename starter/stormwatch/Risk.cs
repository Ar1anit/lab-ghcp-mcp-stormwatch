namespace StormWatch;

public static class StormRiskService
{
    public static (int Score, IReadOnlyList<string> Reasons) ScorePoint(ForecastPoint point)
    {
        // TODO 5: Apply the documented thresholds without double-counting.
        throw new NotImplementedException();
    }

    public static StormAssessment Assess(Forecast forecast)
    {
        // TODO 5: Select the highest score, with earliest timestamp as tie-breaker.
        throw new NotImplementedException();
    }
}