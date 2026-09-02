namespace StormWatch.Tests;

public sealed class AppTests
{
    [Fact]
    public async Task ReportContainsForecastAssessmentEvidenceAndDisclaimer()
    {
        Forecast forecast = await WeatherParser.LoadFixtureAsync(
            TestData.FixturePath,
            TestContext.Current.CancellationToken);
        var assessment = new StormAssessment("warning", 100, forecast.Points[2], ["Synthetic reason"]);

        string report = StormWatchApp.RenderReport(forecast, assessment);

        Assert.Contains("Bengaluru, Karnataka, IN", report);
        Assert.Equal(5, CountOccurrences(report, "UTC |"));
        Assert.Contains("WARNING (100/100)", report);
        Assert.Contains("Synthetic reason", report);
        Assert.Contains("educational heuristic", report, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not an official warning", report, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FixtureModeRunsWithoutApiKey()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        int result = await StormWatchApp.RunAsync(
            ["Bengaluru", "--fixture", TestData.FixturePath],
            output,
            error,
            TestContext.Current.CancellationToken);

        Assert.Equal(0, result);
        Assert.Contains("WARNING (100/100)", output.ToString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact]
    public async Task MissingApiKeyIsActionableAndDoesNotCrash()
    {
        string? original = Environment.GetEnvironmentVariable("OPENWEATHER_API_KEY");
        Environment.SetEnvironmentVariable("OPENWEATHER_API_KEY", null);
        try
        {
            var output = new StringWriter();
            var error = new StringWriter();

            int result = await StormWatchApp.RunAsync(
                ["Bengaluru"],
                output,
                error,
                TestContext.Current.CancellationToken);

            Assert.Equal(2, result);
            Assert.Equal(string.Empty, output.ToString());
            Assert.Contains("OPENWEATHER_API_KEY", error.ToString());
        }
        finally
        {
            Environment.SetEnvironmentVariable("OPENWEATHER_API_KEY", original);
        }
    }

    private static int CountOccurrences(string value, string search) =>
        (value.Length - value.Replace(search, string.Empty, StringComparison.Ordinal).Length)
        / search.Length;
}