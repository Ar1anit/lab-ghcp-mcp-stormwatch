using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace StormWatch;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        string[] appArgs = LoadOpenWeatherEnvironment(args);
        if (!appArgs.Contains("--mcp", StringComparer.Ordinal))
        {
            return await StormWatchApp.RunAsync(appArgs);
        }

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(appArgs);
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<StormWatchDataService>();
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();
        await builder.Build().RunAsync();
        return 0;
    }

    private static string[] LoadOpenWeatherEnvironment(string[] args)
    {
        var appArgs = new List<string>();
        for (int index = 0; index < args.Length; index++)
        {
            if (args[index] != "--env-file")
            {
                appArgs.Add(args[index]);
                continue;
            }

            if (index + 1 >= args.Length)
            {
                throw new ArgumentException("--env-file requires a path.");
            }

            LoadOpenWeatherKey(args[++index]);
        }

        return [.. appArgs];
    }

    private static void LoadOpenWeatherKey(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("The OpenWeather environment file was not found.", path);
        }

        foreach (string line in File.ReadLines(path))
        {
            string trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            int separator = trimmed.IndexOf('=');
            if (separator <= 0 ||
                trimmed[..separator].Trim() != "OPENWEATHER_API_KEY")
            {
                continue;
            }

            string value = trimmed[(separator + 1)..].Trim().Trim('"', '\'');
            if (value.Length > 0)
            {
                Environment.SetEnvironmentVariable("OPENWEATHER_API_KEY", value);
            }
        }
    }
}