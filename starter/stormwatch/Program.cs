using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace StormWatch;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        if (!args.Contains("--mcp", StringComparer.Ordinal))
        {
            return await StormWatchApp.RunAsync(args);
        }

        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<StormWatchDataService>();
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithToolsFromAssembly();
        await builder.Build().RunAsync();
        return 0;
    }
}