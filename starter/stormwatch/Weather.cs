using System.Text.Json;

namespace StormWatch;

public sealed class WeatherServiceException(string message, Exception? innerException = null)
    : Exception(message, innerException);

public sealed class OpenWeatherClient
{
    public const string GeocodingUrl = "https://api.openweathermap.org/geo/1.0/direct";
    public const string ForecastUrl = "https://api.openweathermap.org/data/2.5/forecast";

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly TimeSpan _timeout;

    public OpenWeatherClient(HttpClient httpClient, string apiKey, TimeSpan? timeout = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        _httpClient = httpClient;
        _apiKey = apiKey;
        _timeout = timeout ?? TimeSpan.FromSeconds(10);
    }

    public Task<Forecast> GetForecastAsync(
        string city,
        CancellationToken cancellationToken = default)
    {
        // TODO 1: Geocode the city, request metric forecast data, and parse it.
        throw new NotImplementedException();
    }
}

public static class WeatherParser
{
    public static Location ParseLocation(JsonElement payload)
    {
        // TODO 1: Validate and map the first direct-geocoding result.
        throw new NotImplementedException();
    }

    public static Forecast ParseForecast(JsonElement payload, Location location)
    {
        // TODO 1: Map forecast list entries and default missing rain to zero.
        throw new NotImplementedException();
    }

    public static async Task<Forecast> LoadFixtureAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        await using FileStream stream = File.OpenRead(path);
        using JsonDocument fixture = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);
        JsonElement root = fixture.RootElement;
        Location location = ParseLocation(root.GetProperty("geocoding"));
        return ParseForecast(root.GetProperty("forecast"), location);
    }
}