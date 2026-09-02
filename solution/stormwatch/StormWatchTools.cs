using System.ComponentModel;
using ModelContextProtocol.Server;

namespace StormWatch;

public sealed class StormWatchDataService
{
    public async Task<(Forecast Forecast, string Source)> LoadAsync(
        string city,
        CancellationToken cancellationToken)
    {
        string? fixturePath = Environment.GetEnvironmentVariable("STORMWATCH_FIXTURE_PATH");
        Forecast forecast = await StormWatchApp.LoadForecastAsync(
            city,
            fixturePath,
            cancellationToken);
        string source = fixturePath is null
            ? "OpenWeather 5 Day / 3 Hour Forecast"
            : "synthetic fixture";
        return (forecast, source);
    }
}

[McpServerToolType]
public sealed class StormWatchTools(StormWatchDataService dataService)
{
    [McpServerTool(Name = "get_forecast"), Description(
        "Get the next five OpenWeather forecast points for a city.")]
    public async Task<ForecastToolResult> GetForecastAsync(
        [Description("City name, optionally followed by a country code")] string city,
        CancellationToken cancellationToken)
    {
        (Forecast forecast, string source) = await dataService.LoadAsync(city, cancellationToken);
        return new ForecastToolResult(
            forecast.Location.DisplayName,
            new Coordinates(forecast.Location.Latitude, forecast.Location.Longitude),
            source,
            new Units("C", "m/s", "mm/3h", "hPa"),
            forecast.Points.Take(5).Select(point => new ForecastPeriod(
                point.Timestamp,
                point.TemperatureC,
                point.Description,
                point.WeatherId,
                point.WindSpeedMps,
                point.Rain3hMm,
                point.PressureHpa)).ToArray());
    }

    [McpServerTool(Name = "assess_storm_risk"), Description(
        "Assess an educational storm-risk signal for a city's forecast.")]
    public async Task<AssessmentToolResult> AssessStormRiskAsync(
        [Description("City name, optionally followed by a country code")] string city,
        CancellationToken cancellationToken)
    {
        (Forecast forecast, string source) = await dataService.LoadAsync(city, cancellationToken);
        StormAssessment assessment = StormRiskService.Assess(forecast);
        return new AssessmentToolResult(
            forecast.Location.DisplayName,
            source,
            assessment.Level,
            assessment.Score,
            assessment.Peak.Timestamp,
            assessment.Reasons,
            "Educational heuristic only; not an official warning or reliable storm prediction.");
    }
}

public sealed record Coordinates(double Latitude, double Longitude);
public sealed record Units(string Temperature, string WindSpeed, string Rain, string Pressure);
public sealed record ForecastPeriod(
    DateTimeOffset TimeUtc,
    double TemperatureC,
    string Description,
    int WeatherId,
    double WindSpeedMps,
    double Rain3hMm,
    int PressureHpa);
public sealed record ForecastToolResult(
    string Location,
    Coordinates Coordinates,
    string Source,
    Units Units,
    IReadOnlyList<ForecastPeriod> Periods);
public sealed record AssessmentToolResult(
    string Location,
    string Source,
    string Level,
    int Score,
    DateTimeOffset PeakTimeUtc,
    IReadOnlyList<string> Evidence,
    string Disclaimer);