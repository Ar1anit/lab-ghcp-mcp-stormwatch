using System.Net;
using System.Text;

namespace StormWatch.Tests;

internal static class TestData
{
    public static string FixturePath => Path.Combine(
        AppContext.BaseDirectory,
        "fixtures",
        "forecast.json");

    public static readonly Location Location = new("Test City", "TC", 12.0, 77.0);

    public static ForecastPoint Point(
        int hour = 0,
        int weatherId = 800,
        double windSpeedMps = 0.0,
        double rain3hMm = 0.0,
        int pressureHpa = 1013) => new(
            new DateTimeOffset(2026, 8, 30, hour, 0, 0, TimeSpan.Zero),
            24.0,
            weatherId,
            "synthetic weather",
            windSpeedMps,
            rain3hMm,
            pressureHpa);
}

internal sealed class QueueHttpMessageHandler(params HttpResponseMessage[] responses)
    : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses = new(responses);
    public List<HttpRequestMessage> Requests { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(_responses.Dequeue());
    }

    public static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) =>
        new(status)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
}