using System.Globalization;

namespace StormWatch;

public static class StormRiskService
{
    public static (int Score, IReadOnlyList<string> Reasons) ScorePoint(ForecastPoint point)
    {
        int score = 0;
        var reasons = new List<string>();

        if (point.WeatherId is >= 200 and <= 232)
        {
            score += 60;
            reasons.Add($"Thunderstorm condition code {point.WeatherId} (+60).");
        }

        if (point.WindSpeedMps >= 15.0)
        {
            score += 25;
            reasons.Add($"Wind {point.WindSpeedMps.ToString("F1", CultureInfo.InvariantCulture)} m/s is at least 15 m/s (+25).");
        }
        else if (point.WindSpeedMps >= 10.0)
        {
            score += 15;
            reasons.Add($"Wind {point.WindSpeedMps.ToString("F1", CultureInfo.InvariantCulture)} m/s is at least 10 m/s (+15).");
        }

        if (point.Rain3hMm >= 10.0)
        {
            score += 20;
            reasons.Add($"Rain {point.Rain3hMm.ToString("F1", CultureInfo.InvariantCulture)} mm/3h is at least 10 mm/3h (+20).");
        }
        else if (point.Rain3hMm >= 5.0)
        {
            score += 10;
            reasons.Add($"Rain {point.Rain3hMm.ToString("F1", CultureInfo.InvariantCulture)} mm/3h is at least 5 mm/3h (+10).");
        }

        if (point.PressureHpa <= 990)
        {
            score += 15;
            reasons.Add($"Pressure {point.PressureHpa} hPa is at most 990 hPa (+15).");
        }
        else if (point.PressureHpa <= 1000)
        {
            score += 8;
            reasons.Add($"Pressure {point.PressureHpa} hPa is at most 1000 hPa (+8).");
        }

        return (Math.Min(score, 100), reasons);
    }

    public static StormAssessment Assess(Forecast forecast)
    {
        if (forecast.Points.Count == 0)
        {
            throw new ArgumentException("Cannot assess an empty forecast.", nameof(forecast));
        }

        var peak = forecast.Points
            .Select(point => (Point: point, Result: ScorePoint(point)))
            .OrderByDescending(item => item.Result.Score)
            .ThenBy(item => item.Point.Timestamp)
            .First();
        string level = peak.Result.Score switch
        {
            >= 60 => "warning",
            >= 30 => "watch",
            _ => "low",
        };
        return new StormAssessment(level, peak.Result.Score, peak.Point, peak.Result.Reasons);
    }
}