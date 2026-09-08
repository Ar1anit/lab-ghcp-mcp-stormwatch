namespace StormWatch.Chat;

public sealed record ChatRuntimeOptions(
    bool UseRag,
    bool UseTools,
    string ServerProject,
    string WeatherEnvironmentFile,
    string? FixturePath)
{
    public static ChatRuntimeOptions Parse(string[] args, string workingDirectory)
    {
        bool useRag = true;
        bool useTools = false;
        string? fixturePath = null;
        string serverProject = Path.Combine(
            workingDirectory,
            "stormwatch",
            "StormWatch.csproj");

        for (int index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--no-rag":
                    useRag = false;
                    break;
                case "--with-tools":
                    useTools = true;
                    break;
                case "--fixture" when index + 1 < args.Length:
                    fixturePath = Path.GetFullPath(args[++index], workingDirectory);
                    break;
                case "--server-project" when index + 1 < args.Length:
                    serverProject = Path.GetFullPath(args[++index], workingDirectory);
                    break;
                default:
                    throw new ArgumentException(
                        "Usage: StormWatch.Chat [--no-rag] [--with-tools] " +
                        "[--fixture <path>] [--server-project <path>]");
            }
        }

        if (fixturePath is not null && !useTools)
        {
            throw new ArgumentException("--fixture requires --with-tools.");
        }

        return new ChatRuntimeOptions(
            useRag,
            useTools,
            serverProject,
            Path.Combine(workingDirectory, ".env.openweather"),
            fixturePath);
    }
}