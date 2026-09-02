namespace StormWatch.Tests;

public sealed class RiskTests
{
    [Fact]
    public void ThunderstormCodeRangeIsInclusive()
    {
        foreach (int weatherId in new[] { 200, 232 })
        {
            (int score, IReadOnlyList<string> reasons) = StormRiskService.ScorePoint(
                TestData.Point(weatherId: weatherId));
            Assert.Equal(60, score);
            Assert.Contains(reasons, reason => reason.Contains("thunderstorm", StringComparison.OrdinalIgnoreCase));
        }

        Assert.Equal(0, StormRiskService.ScorePoint(TestData.Point(weatherId: 199)).Score);
        Assert.Equal(0, StormRiskService.ScorePoint(TestData.Point(weatherId: 233)).Score);
    }

    [Fact]
    public void WindThresholdsDoNotDoubleCount()
    {
        foreach ((double wind, int expected) in new[] { (9.9, 0), (10.0, 15), (14.9, 15), (15.0, 25) })
        {
            Assert.Equal(expected, StormRiskService.ScorePoint(TestData.Point(windSpeedMps: wind)).Score);
        }
    }

    [Fact]
    public void RainThresholdsDoNotDoubleCount()
    {
        foreach ((double rain, int expected) in new[] { (4.9, 0), (5.0, 10), (9.9, 10), (10.0, 20) })
        {
            Assert.Equal(expected, StormRiskService.ScorePoint(TestData.Point(rain3hMm: rain)).Score);
        }
    }

    [Fact]
    public void PressureThresholdsDoNotDoubleCount()
    {
        foreach ((int pressure, int expected) in new[] { (1001, 0), (1000, 8), (991, 8), (990, 15) })
        {
            Assert.Equal(expected, StormRiskService.ScorePoint(TestData.Point(pressureHpa: pressure)).Score);
        }
    }

    [Fact]
    public void CombinedScoreIsCappedAndAllIndicatorsAreExplained()
    {
        (int score, IReadOnlyList<string> reasons) = StormRiskService.ScorePoint(TestData.Point(
            weatherId: 211,
            windSpeedMps: 15.2,
            rain3hMm: 12.4,
            pressureHpa: 988));

        Assert.Equal(100, score);
        Assert.Equal(4, reasons.Count);
    }

    [Fact]
    public void AssignsLevelsAtBoundaries()
    {
        var cases = new[]
        {
            (TestData.Point(), "low", 0),
            (TestData.Point(windSpeedMps: 10.0, pressureHpa: 1000), "low", 23),
            (TestData.Point(windSpeedMps: 15.0, rain3hMm: 5.0), "watch", 35),
            (TestData.Point(weatherId: 200), "warning", 60),
        };
        foreach ((ForecastPoint point, string level, int score) in cases)
        {
            StormAssessment assessment = StormRiskService.Assess(new Forecast(TestData.Location, [point]));
            Assert.Equal(level, assessment.Level);
            Assert.Equal(score, assessment.Score);
        }
    }

    [Fact]
    public void SelectsEarliestPointWhenPeakScoresTie()
    {
        ForecastPoint later = TestData.Point(hour: 6, weatherId: 200);
        ForecastPoint earlier = TestData.Point(hour: 3, weatherId: 200);

        StormAssessment assessment = StormRiskService.Assess(
            new Forecast(TestData.Location, [later, earlier]));

        Assert.Equal(earlier, assessment.Peak);
    }

    [Fact]
    public void RejectsEmptyForecast()
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(
            () => StormRiskService.Assess(new Forecast(TestData.Location, [])));
        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}