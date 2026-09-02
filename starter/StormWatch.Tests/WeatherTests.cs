using System.Net;
using System.Text.Json;

namespace StormWatch.Tests;

public sealed class WeatherTests
{
    [Fact]
    public async Task ParsesLocation()
    {
        using JsonDocument fixture = await LoadFixtureAsync();
        Location location = WeatherParser.ParseLocation(fixture.RootElement.GetProperty("geocoding"));

        Assert.Equal("Bengaluru, Karnataka, IN", location.DisplayName);
        Assert.Equal(12.9767936, location.Latitude, 7);
        Assert.Equal(77.590082, location.Longitude, 6);
    }

    [Fact]
    public void RejectsUnknownLocation()
    {
        using JsonDocument payload = JsonDocument.Parse("[]");
        WeatherServiceException exception = Assert.Throws<WeatherServiceException>(
            () => WeatherParser.ParseLocation(payload.RootElement));
        Assert.Contains("not found", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ParsesForecastAndDefaultsMissingRain()
    {
        using JsonDocument fixture = await LoadFixtureAsync();
        Location location = WeatherParser.ParseLocation(fixture.RootElement.GetProperty("geocoding"));
        Forecast forecast = WeatherParser.ParseForecast(
            fixture.RootElement.GetProperty("forecast"),
            location);

        Assert.Equal(5, forecast.Points.Count);
        Assert.Equal("2026-08-30T00:00:00.0000000+00:00", forecast.Points[0].Timestamp.ToString("O"));
        Assert.Equal(0.0, forecast.Points[0].Rain3hMm);
        Assert.Equal(211, forecast.Points[2].WeatherId);
        Assert.Equal(15.2, forecast.Points[2].WindSpeedMps);
        Assert.Equal(988, forecast.Points[2].PressureHpa);
    }

    [Fact]
    public async Task LoadsCombinedOfflineFixture()
    {
        Forecast forecast = await WeatherParser.LoadFixtureAsync(
            TestData.FixturePath,
            TestContext.Current.CancellationToken);

        Assert.Equal("Bengaluru", forecast.Location.Name);
        Assert.Equal(5, forecast.Points.Count);
    }

    [Fact]
    public async Task GeocodesCityThenRequestsMetricForecast()
    {
        using JsonDocument fixture = await LoadFixtureAsync();
        string geocoding = fixture.RootElement.GetProperty("geocoding").GetRawText();
        string forecastJson = fixture.RootElement.GetProperty("forecast").GetRawText();
        var handler = new QueueHttpMessageHandler(
            QueueHttpMessageHandler.Json(geocoding),
            QueueHttpMessageHandler.Json(forecastJson));
        using var client = new HttpClient(handler);

        Forecast forecast = await new OpenWeatherClient(client, "test-key", TimeSpan.FromSeconds(4))
            .GetForecastAsync("Bengaluru", TestContext.Current.CancellationToken);

        Assert.Equal("IN", forecast.Location.Country);
        Assert.Equal(2, handler.Requests.Count);
        string geocodingQuery = handler.Requests[0].RequestUri!.Query;
        Assert.Contains("q=Bengaluru", geocodingQuery);
        Assert.Contains("limit=1", geocodingQuery);
        Assert.Contains("appid=test-key", geocodingQuery);
        string forecastQuery = handler.Requests[1].RequestUri!.Query;
        Assert.Contains("lat=12.9767936", forecastQuery);
        Assert.Contains("lon=77.590082", forecastQuery);
        Assert.Contains("units=metric", forecastQuery);
        Assert.DoesNotContain("q=", forecastQuery);
    }

    [Fact]
    public async Task RedactsApiKeyFromHttpErrors()
    {
        var handler = new QueueHttpMessageHandler(
            QueueHttpMessageHandler.Json("{}", HttpStatusCode.Unauthorized));
        using var client = new HttpClient(handler);

        WeatherServiceException exception = await Assert.ThrowsAsync<WeatherServiceException>(
            () => new OpenWeatherClient(client, "top-secret").GetForecastAsync(
                "Bengaluru",
                TestContext.Current.CancellationToken));

        Assert.Contains("HTTP 401", exception.Message);
        Assert.DoesNotContain("top-secret", exception.ToString());
    }

    private static async Task<JsonDocument> LoadFixtureAsync()
    {
        await using FileStream stream = File.OpenRead(TestData.FixturePath);
        return await JsonDocument.ParseAsync(stream);
    }
}