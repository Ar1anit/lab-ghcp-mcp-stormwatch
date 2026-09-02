namespace StormWatch;

public sealed record Location(
    string Name,
    string Country,
    double Latitude,
    double Longitude,
    string? State = null)
{
    public string DisplayName => string.Join(", ", new[] { Name, State, Country }
        .Where(part => !string.IsNullOrWhiteSpace(part)));
}

public sealed record ForecastPoint(
    DateTimeOffset Timestamp,
    double TemperatureC,
    int WeatherId,
    string Description,
    double WindSpeedMps,
    double Rain3hMm,
    int PressureHpa);

public sealed record Forecast(Location Location, IReadOnlyList<ForecastPoint> Points);

public sealed record StormAssessment(
    string Level,
    int Score,
    ForecastPoint Peak,
    IReadOnlyList<string> Reasons);