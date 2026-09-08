# Task 2: Build a Conversational Chatbot

**Timebox:** 14 minutes  
**Clock:** 0:08-0:22  
**Primary file:** `StormWatch.Chat/ChatSession.cs`

## Outcome

Turn the model client into an interactive, multi-turn console chatbot. This task
uses the Foundry model only; retrieval and MCP tools remain disabled.

## Step 1: Select Focused Context

Open:

- `StormWatch.Chat/Program.cs`;
- `StormWatch.Chat/ChatSession.cs`;
- `StormWatch.Chat/ChatRuntimeOptions.cs`; and
- the two Task 2 tests in `StormWatch.Chat.Tests/ChatTests.cs`.

The existing `FoundryChatCompletionService` owns SDK calls. Complete only
`GroundedChatSession.AskAsync` and `ConsoleChatRunner.RunAsync`.

## Step 2: Define the Conversation Contract

Your implementation must:

1. reject blank questions at the session boundary;
2. reject questions longer than `MaxQuestionLength`;
3. send the system message and prior turns with each model request;
4. store the original user question and assistant answer after a successful
   completion;
5. prompt repeatedly until `/exit` or end-of-input;
6. preserve cancellation; and
7. show safe, concise turn failures without printing a stack trace or secret.

When the retriever returns no passages, send the original question directly.
That rule keeps this model-only stage independent of Task 4 grounding.

## Step 3: Write and Review a Focused Prompt

Ask Copilot to implement the two Task 2 TODOs without changing the retrieval,
grounding, weather, or MCP types. Review for an unbounded history mutation,
assistant messages added before a successful response, swallowed cancellation,
and accidental credential output.

## Step 4: Run Focused Tests

```powershell
dotnet test .\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj `
  --filter "FullyQualifiedName~ConsoleRunner|FullyQualifiedName~ChatSessionRetainsHistoryWithoutRetrieval"
```

Both tests must pass. The local-vector and grounded-source tests should remain
red.

## Step 5: Talk to the Chatbot

Run without RAG or tools:

```powershell
dotnet run --project .\StormWatch.Chat -- --no-rag
```

Have at least two related turns, then enter `/exit`. For example, establish a
topic in one turn and use a pronoun in the next. The second response should
reflect retained history. Do not ask for live weather yet; the model has no
weather tool in this mode.

## Completion Gate

- [ ] Both focused tests pass with no warnings.
- [ ] A two-turn console conversation completes and `/exit` returns normally.
- [ ] The second model request contains the first user and assistant turns.
- [ ] Model-only mode does not build the local vector index or start the MCP
  server.
- [ ] No credential, raw exception, or stack trace appears in output.
- [ ] Only Task 2 TODOs changed.

**Evidence to retain:** focused test summary and a credential-free two-turn
transcript.

Continue to [Task 3: Build a local vector index](03-build-local-vector-index.md).
