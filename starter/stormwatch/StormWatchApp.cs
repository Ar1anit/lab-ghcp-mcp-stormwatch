namespace StormWatch;

public static class StormWatchApp
{
    public static Task<int> RunAsync(
        string[] args,
        TextWriter? output = null,
        TextWriter? error = null,
        CancellationToken cancellationToken = default)
    {
        // TODO 3: Parse CLI arguments, load data, assess risk, and report errors.
        throw new NotImplementedException();
    }

    public static Task<Forecast> LoadForecastAsync(
        string city,
        string? fixturePath,
        CancellationToken cancellationToken = default)
    {
        // TODO 3: Prefer the fixture; otherwise require OPENWEATHER_API_KEY.
        throw new NotImplementedException();
    }

    public static string RenderReport(Forecast forecast, StormAssessment assessment)
    {
        // TODO 3: Render five forecast points, evidence, and the disclaimer.
        throw new NotImplementedException();
    }
}