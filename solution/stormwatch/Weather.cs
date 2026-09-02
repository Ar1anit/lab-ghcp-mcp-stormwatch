using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
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

    public async Task<Forecast> GetForecastAsync(
        string city,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        string normalizedCity = city.Trim();
        using JsonDocument geocoding = await RequestJsonAsync(
            GeocodingUrl,
            new Dictionary<string, string>
            {
                ["q"] = normalizedCity,
                ["limit"] = "1",
                ["appid"] = _apiKey,
            },
            cancellationToken);
        Location location = WeatherParser.ParseLocation(geocoding.RootElement);

        using JsonDocument forecast = await RequestJsonAsync(
            ForecastUrl,
            new Dictionary<string, string>
            {
                ["lat"] = location.Latitude.ToString(CultureInfo.InvariantCulture),
                ["lon"] = location.Longitude.ToString(CultureInfo.InvariantCulture),
                ["appid"] = _apiKey,
                ["units"] = "metric",
            },
            cancellationToken);
        return WeatherParser.ParseForecast(forecast.RootElement, location);
    }

    private async Task<JsonDocument> RequestJsonAsync(
        string baseUrl,
        IReadOnlyDictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        string query = string.Join("&", parameters.Select(pair =>
            $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}?{query}");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd("StormWatch-Lab/1.0");
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(_timeout);

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, timeoutSource.Token);
            if (!response.IsSuccessStatusCode)
            {
                throw new WeatherServiceException(
                    $"OpenWeather request failed with HTTP {(int)response.StatusCode}.");
            }

            await using Stream body = await response.Content.ReadAsStreamAsync(timeoutSource.Token);
            return await JsonDocument.ParseAsync(body, cancellationToken: timeoutSource.Token);
        }
        catch (WeatherServiceException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new WeatherServiceException("OpenWeather returned invalid JSON.", exception);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new WeatherServiceException("OpenWeather request timed out.", exception);
        }
        catch (HttpRequestException exception)
        {
            throw new WeatherServiceException("OpenWeather could not be reached.", exception);
        }
    }
}

public static class WeatherParser
{
    public static Location ParseLocation(JsonElement payload)
    {
        try
        {
            if (payload.ValueKind != JsonValueKind.Array || payload.GetArrayLength() == 0)
            {
                throw new WeatherServiceException("Location not found by OpenWeather.");
            }

            JsonElement result = payload[0];
            return new Location(
                result.GetProperty("name").GetString()
                    ?? throw new JsonException(),
                result.GetProperty("country").GetString()
                    ?? throw new JsonException(),
                result.GetProperty("lat").GetDouble(),
                result.GetProperty("lon").GetDouble(),
                result.TryGetProperty("state", out JsonElement state)
                    ? state.GetString()
                    : null);
        }
        catch (WeatherServiceException)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            throw new WeatherServiceException(
                "OpenWeather returned an unexpected location response.", exception);
        }
    }

    public static Forecast ParseForecast(JsonElement payload, Location location)
    {
        try
        {
            if (!payload.TryGetProperty("list", out JsonElement list)
                || list.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException();
            }

            var points = new List<ForecastPoint>();
            foreach (JsonElement item in list.EnumerateArray())
            {
                JsonElement main = item.GetProperty("main");
                JsonElement weather = item.GetProperty("weather")[0];
                JsonElement wind = item.GetProperty("wind");
                double rain = item.TryGetProperty("rain", out JsonElement rainObject)
                    && rainObject.TryGetProperty("3h", out JsonElement rain3h)
                    ? rain3h.GetDouble()
                    : 0.0;
                points.Add(new ForecastPoint(
                    DateTimeOffset.FromUnixTimeSeconds(item.GetProperty("dt").GetInt64()),
                    main.GetProperty("temp").GetDouble(),
                    weather.GetProperty("id").GetInt32(),
                    weather.GetProperty("description").GetString()
                        ?? throw new JsonException(),
                    wind.GetProperty("speed").GetDouble(),
                    rain,
                    main.GetProperty("pressure").GetInt32()));
            }

            if (points.Count == 0)
            {
                throw new WeatherServiceException("OpenWeather returned an empty forecast.");
            }

            return new Forecast(location, points);
        }
        catch (WeatherServiceException)
        {
            throw;
        }
        catch (Exception exception) when (exception is JsonException or InvalidOperationException)
        {
            throw new WeatherServiceException(
                "OpenWeather returned an unexpected forecast response.", exception);
        }
    }

    public static async Task<Forecast> LoadFixtureAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using FileStream stream = File.OpenRead(path);
            using JsonDocument fixture = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);
            JsonElement root = fixture.RootElement;
            Location location = ParseLocation(root.GetProperty("geocoding"));
            return ParseForecast(root.GetProperty("forecast"), location);
        }
        catch (WeatherServiceException)
        {
            throw;
        }
        catch (Exception exception) when (exception is IOException
            or JsonException
            or InvalidOperationException)
        {
            throw new WeatherServiceException("The forecast fixture is invalid.", exception);
        }
    }
}