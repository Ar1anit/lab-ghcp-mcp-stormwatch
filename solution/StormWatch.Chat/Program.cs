using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using StormWatch.Chat;

EnvironmentFile.Load(Path.Combine(Environment.CurrentDirectory, ".env"));

ChatRuntimeOptions options;
try
{
    options = ChatRuntimeOptions.Parse(args, Environment.CurrentDirectory);
}
catch (ArgumentException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 2;
}

string endpoint = RequireEnvironmentVariable("FOUNDRY_MODEL_ENDPOINT");
string deployment = RequireEnvironmentVariable("FOUNDRY_MODEL_DEPLOYMENT");
string apiKey = RequireEnvironmentVariable("FOUNDRY_MODEL_API_KEY");
var foundryClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));

IChatClient chatClient = new ChatClientBuilder(
    foundryClient.GetChatClient(deployment)
            .AsIChatClient())
    .UseFunctionInvocation()
    .Build();

using var shutdown = new CancellationTokenSource();
ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};
Console.CancelKeyPress += cancelHandler;

McpToolSession? toolSession = null;
try
{
    IKnowledgeRetriever retriever = EmptyKnowledgeRetriever.Instance;
    if (options.UseRag)
    {
        string embeddingDeployment = RequireEnvironmentVariable(
            "FOUNDRY_EMBEDDING_DEPLOYMENT");
        retriever = await LocalVectorKnowledgeRetriever.CreateAsync(
            LocalRagSettings.FromWorkingDirectory(Environment.CurrentDirectory),
            new ModelEmbeddingService(
                foundryClient.GetEmbeddingClient(embeddingDeployment)
                    .AsIEmbeddingGenerator()),
            shutdown.Token);
    }

    IReadOnlyList<AITool> tools = [];
    if (options.UseTools)
    {
        toolSession = await McpToolSession.ConnectAsync(
            options.ServerProject,
            options.WeatherEnvironmentFile,
            options.FixturePath,
            shutdown.Token);
        tools = [.. toolSession.Tools];
    }

    var completionService = new FoundryChatCompletionService(chatClient);
    var session = new GroundedChatSession(completionService, retriever, tools);
    return await ConsoleChatRunner.RunAsync(
        session,
        Console.In,
        Console.Out,
        shutdown.Token);
}
catch (OperationCanceledException) when (shutdown.IsCancellationRequested)
{
    return 130;
}
finally
{
    Console.CancelKeyPress -= cancelHandler;
    if (toolSession is not null)
    {
        await toolSession.DisposeAsync();
    }
}

static string RequireEnvironmentVariable(string name)
{
    string? value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Set {name} before running the chatbot.");
    }

    return value;
}