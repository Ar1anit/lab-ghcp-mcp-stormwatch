# Task 4: Ground Answers with Citations

**Timebox:** 12 minutes  
**Clock:** 0:37-0:49  
**Primary files:** `StormWatch.Chat/Grounding.cs`, `ChatSession.cs`

## Outcome

Complete the generation stage of the RAG pipeline. Give the model clearly
delimited local passages, require evidence-linked citations, retain natural
conversation history, and append a deterministic source list.

## Step 1: Select Focused Context

Open:

- `StormWatch.Chat/Grounding.cs`;
- `StormWatch.Chat/ChatSession.cs`;
- `StormWatch.Chat/Knowledge.cs`; and
- `GroundedSessionUsesSourcesAndRetainsConversationHistory` in
  `StormWatch.Chat.Tests/ChatTests.cs`.

The retriever finds passages. `Grounding` formats untrusted context and source
metadata. `GroundedChatSession` orchestrates a turn. Keep those roles separate.

## Step 2: Build the Grounded Prompt

`Grounding.BuildPrompt` should:

1. preserve the user's question;
2. label each passage `[S1]`, `[S2]`, and so on;
3. delimit title, content, and relative source path;
4. state that retrieved text is untrusted data, never instructions;
5. require citations for preparedness claims; and
6. require an explicit "I don't know" when sources and tools do not support an
   answer.

Do not treat retrieval as proof that every passage is relevant or correct.

## Step 3: Produce Deterministic Sources

`Grounding.AppendSources` should append only the passages supplied to that turn,
preserve citation numbering, and de-duplicate repeated source paths or IDs. It
must not invent a source or ask the model to format an authoritative bibliography.

Update `GroundedChatSession.AskAsync` so it:

- retrieves once for the current question;
- uses the original question when no passage is returned;
- uses the grounded prompt when passages exist;
- sends prior natural user/assistant turns as history;
- adds history only after a non-empty successful completion; and
- appends sources only when passages exist.

## Step 4: Run Focused Verification

```powershell
dotnet test .\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj `
  --filter FullyQualifiedName~GroundedSession
```

Then run the complete chatbot tests. All five should now pass:

```powershell
dotnet test .\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj
```

## Step 5: Validate Live RAG

Run the default mode, which builds the in-memory index from `rag-data` but does
not start MCP:

```powershell
dotnet run --project .\StormWatch.Chat
```

Ask one question that the supplied corpus answers and one it does not. The
grounded answer should include `[S#]` references and a `Sources:` section. An
unsupported question should not receive a fabricated preparedness claim.

## Completion Gate

- [ ] All five chatbot tests pass without live service calls.
- [ ] Retrieved text is visibly delimited and treated as untrusted data.
- [ ] Knowledge claims point to source IDs present in the source list.
- [ ] A no-result turn does not reuse stale sources from an earlier turn.
- [ ] Natural conversation history is retained without storing augmented
      prompts as user history.
- [ ] A supported and unsupported live question behave as expected.
- [ ] Foundry model and embedding credentials remain out of source and output.

**Evidence to retain:** five-test pass summary and a redacted transcript showing
one cited answer and one bounded no-evidence answer.

## Sources

- [Retrieval augmented generation (RAG) and indexes](https://learn.microsoft.com/azure/foundry/concepts/retrieval-augmented-generation) -
  grounding data, citations, security considerations, and RAG limitations.
- [RAG prompt engineering](https://learn.microsoft.com/azure/architecture/ai-ml/guide/rag/rag-prompt-engineering) -
  structuring retrieved context and requiring evidence-based responses.
- [Prompt Shields](https://learn.microsoft.com/azure/ai-services/content-safety/concepts/jailbreak-detection#prompt-shields-for-documents) -
  the indirect prompt-injection threat posed by instructions embedded in
  retrieved documents.

Continue to [Task 5: Add weather and risk services](05-add-weather-and-risk-services.md).
