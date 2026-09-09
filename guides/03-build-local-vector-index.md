# Task 3: Build a Local Vector Index

**Timebox:** 15 minutes  
**Clock:** 0:22-0:37  
**Primary file:** `StormWatch.Chat/Knowledge.cs`

## Outcome

Build the retrieval stage of the RAG pipeline. Load the facilitator-provided
`.md` and `.txt` files from `rag-data`, split them into bounded overlapping
chunks, create embeddings with the supplied model, store the vectors in memory,
and return the three most similar chunks for a question.

Do not generate an answer in this task. No Azure AI Search service or external
vector database is used.

## Step 1: Confirm the Supplied Contract

The facilitator places approved educational files under:

```text
rag-data/
  ... .md and .txt files
```

The existing private `.env` also contains the embedding deployment name:

```dotenv
FOUNDRY_EMBEDDING_DEPLOYMENT=<embedding-deployment-name>
```

Do not open or print the populated environment file. The application sends
document chunks and questions to the facilitator-provided embedding deployment,
but source files and the resulting vector index remain on the participant
machine. Do not use restricted, personal, patient, or operational data.

## Step 2: Select Focused Context

Open:

- `StormWatch.Chat/Knowledge.cs`;
- `StormWatch.Chat/Program.cs`;
- `.env.example`; and
- `LocalVectorRetrieverLoadsChunksAndRanksByCosineSimilarity` and
  `LocalRagSettingsUseRagDataFolderAndOptionsIsolateSecrets` in
  `StormWatch.Chat.Tests/ChatTests.cs`.

The supplied loader accepts only `.md` and `.txt`, caps file and corpus sizes,
creates relative source paths, and chunks text with overlap. The supplied
`ModelEmbeddingService` owns the SDK call. Implement only
`LocalVectorKnowledgeRetriever.CreateAsync` and `RetrieveAsync`.

## Step 3: Build the In-Memory Index

`CreateAsync` should:

1. load the prepared chunks with `LoadPassagesAsync`;
2. send only chunk text to `ITextEmbeddingService` in one batch;
3. verify that every chunk receives one vector;
4. normalize each vector with the supplied helper; and
5. retain each normalized vector beside its `KnowledgePassage` in memory.

Build the index once when the chatbot starts. Do not regenerate document
embeddings for every question, persist vectors to source control, or log source
content or vectors.

## Step 4: Retrieve by Cosine Similarity

`RetrieveAsync` should:

1. reject a blank question;
2. trim and embed the question once;
3. require exactly one query vector;
4. normalize it and compare it with every indexed vector using `Similarity`;
5. discard scores below `MinimumSimilarity`;
6. order by score descending, then passage ID for deterministic ties; and
7. return at most `ResultCount` passages.

Pass cancellation through both embedding calls. Keep answer generation and
citation formatting outside this class.

## Step 5: Run Focused Verification

```powershell
dotnet test .\StormWatch.Chat.Tests\StormWatch.Chat.Tests.csproj `
  --filter "FullyQualifiedName~LocalVectorRetriever|FullyQualifiedName~LocalRagSettings"
```

Both tests must pass without a live model call. The fake embedding service
proves supported-file filtering, local source paths, bounded chunks, query
embedding, cosine ranking, and credential isolation.

## Step 6: Inspect and Try the Pipeline

```powershell
git diff -- .\StormWatch.Chat\Knowledge.cs
git diff --name-only
dotnet run --project .\StormWatch.Chat
```

At startup, the live command embeds the local corpus once. Each question then
requires one additional embedding. Ask a known-good corpus question and confirm
the answer cites a file under `rag-data`. Task 4 completes the grounding and
source-list behavior if it is still intentionally red.

## Completion Gate

- [ ] Both focused local-RAG tests pass.
- [ ] Only `.md` and `.txt` files under `rag-data` become bounded chunks.
- [ ] Document vectors are built once and kept only in memory.
- [ ] Each question is embedded once and ranked with cosine similarity.
- [ ] Retrieval returns no more than three deterministic local passages.
- [ ] Tests use `ITextEmbeddingService`; no unit test calls a live model.
- [ ] The embedding deployment name and API key remain only in ignored `.env`.
- [ ] You can explain which data remains local and which text is sent to the
      embedding deployment.

**Evidence to retain:** focused test output and a reviewed `Knowledge.cs` diff.

## Sources

- [Use the IEmbeddingGenerator interface](https://learn.microsoft.com/dotnet/ai/iembeddinggenerator) -
  asynchronous batch embedding generation and cancellation in
  `Microsoft.Extensions.AI`.
- [Generate embeddings with Azure OpenAI](https://learn.microsoft.com/azure/foundry/openai/how-to/embeddings) -
  embedding inputs, vectors, limits, and deployment configuration.
- [Relevance in vector search](https://learn.microsoft.com/azure/search/vector-search-ranking) -
  nearest-neighbor ranking and cosine similarity. This lab implements the
  small-corpus scan locally rather than using Azure AI Search.
- [Retrieval augmented generation (RAG) and indexes](https://learn.microsoft.com/azure/foundry/concepts/retrieval-augmented-generation) -
  the wider retrieval workflow and its limitations.

Continue to [Task 4: Ground answers with citations](04-ground-answers-with-citations.md).
