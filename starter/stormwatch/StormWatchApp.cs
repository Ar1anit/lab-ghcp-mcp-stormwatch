using System.Globalization;

namespace StormWatch;

public static class StormWatchApp
{
    public static async Task<int> RunAsync(
        string[] args,
        TextWriter? output = null,
        TextWriter? error = null,
        CancellationToken cancellationToken = default)
    {
        output ??= Console.Out;
        error ??= Console.Error;
        if (args.Length == 0)
        {
            await error.WriteLineAsync("Usage: stormwatch <city> [--fixture <path>]");
            return 2;
        }

        string city = args[0];
        string? fixturePath = null;
        if (args.Length == 3 && args[1] == "--fixture")
        {
            fixturePath = args[2];
        }
        else if (args.Length != 1)
        {
            await error.WriteLineAsync("Usage: stormwatch <city> [--fixture <path>]");
            return 2;
        }

        try
        {
            Forecast forecast = await LoadForecastAsync(city, fixturePath, cancellationToken);
            StormAssessment assessment = StormRiskService.Assess(forecast);
            await output.WriteLineAsync(RenderReport(forecast, assessment));
            return 0;
        }
        catch (Exception exception) when (exception is IOException
            or ArgumentException
            or WeatherServiceException)
        {
            await error.WriteLineAsync($"StormWatch error: {exception.Message}");
            return 2;
        }
    }

    public static async Task<Forecast> LoadForecastAsync(
        string city,
        string? fixturePath,
        CancellationToken cancellationToken = default)
    {
        if (fixturePath is not null)
        {
            return await WeatherParser.LoadFixtureAsync(fixturePath, cancellationToken);
        }

        string apiKey = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY")?.Trim() ?? "";
        if (apiKey.Length == 0)
        {
            throw new WeatherServiceException(
                "OPENWEATHER_API_KEY is not set; provide it in the process environment.");
        }

        using var httpClient = new HttpClient();
        return await new OpenWeatherClient(httpClient, apiKey)
            .GetForecastAsync(city, cancellationToken);
    }

    public static string RenderReport(Forecast forecast, StormAssessment assessment)
    {
        var lines = new List<string>
        {
            $"STORMWATCH | {forecast.Location.DisplayName}",
            "",
            "NEXT FIVE FORECAST POINTS",
        };
        lines.AddRange(forecast.Points.Take(5).Select(point =>
            $"{point.Timestamp.UtcDateTime:yyyy-MM-dd HH:mm} UTC | "
            + $"{point.TemperatureC.ToString("F1", CultureInfo.InvariantCulture)} C | "
            + $"{point.Description} | wind {point.WindSpeedMps.ToString("F1", CultureInfo.InvariantCulture)} m/s | "
            + $"rain {point.Rain3hMm.ToString("F1", CultureInfo.InvariantCulture)} mm/3h | "
            + $"{point.PressureHpa} hPa"));
        lines.AddRange([
            "",
            $"Storm-risk signal: {assessment.Level.ToUpperInvariant()} ({assessment.Score}/100)",
            $"Peak window: {assessment.Peak.Timestamp.UtcDateTime:yyyy-MM-dd HH:mm} UTC",
            "Evidence:",
        ]);
        lines.AddRange(assessment.Reasons.Count > 0
            ? assessment.Reasons.Select(reason => $"- {reason}")
            : ["- No configured storm indicators were present."]);
        lines.AddRange([
            "",
            "Educational heuristic only; not an official warning or reliable storm prediction.",
        ]);
        return string.Join(Environment.NewLine, lines);
    }
}