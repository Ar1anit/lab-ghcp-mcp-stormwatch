using System.ComponentModel;
using ModelContextProtocol.Server;

namespace StormWatch;

public sealed class StormWatchDataService
{
    public Task<(Forecast Forecast, string Source)> LoadAsync(
        string city,
        CancellationToken cancellationToken)
    {
        string? fixturePath = Environment.GetEnvironmentVariable("STORMWATCH_FIXTURE_PATH");
        string source = fixturePath is null
            ? "OpenWeather 5 Day / 3 Hour Forecast"
            : "synthetic fixture";
        return LoadCoreAsync(city, fixturePath, source, cancellationToken);
    }

    private static async Task<(Forecast Forecast, string Source)> LoadCoreAsync(
        string city,
        string? fixturePath,
        string source,
        CancellationToken cancellationToken)
    {
        Forecast forecast = await StormWatchApp.LoadForecastAsync(
            city,
            fixturePath,
            cancellationToken);
        return (forecast, source);
    }
}

[McpServerToolType]
public sealed class StormWatchTools(StormWatchDataService dataService)
{
    [McpServerTool(Name = "get_forecast"), Description(
        "Get the next five OpenWeather forecast points for a city.")]
    public Task<ForecastToolResult> GetForecastAsync(
        [Description("City name, optionally followed by a country code")] string city,
        CancellationToken cancellationToken)
    {
        _ = dataService;
        // TODO 4: Compose the existing application service; do not duplicate parsing.
        throw new NotImplementedException();
    }

    [McpServerTool(Name = "assess_storm_risk"), Description(
        "Assess an educational storm-risk signal for a city's forecast.")]
    public Task<AssessmentToolResult> AssessStormRiskAsync(
        [Description("City name, optionally followed by a country code")] string city,
        CancellationToken cancellationToken)
    {
        _ = dataService;
        // TODO 4: Compose the existing risk service; return structured evidence.
        throw new NotImplementedException();
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