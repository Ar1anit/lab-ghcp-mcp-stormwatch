# StormWatch Model and Embedding Setup

The facilitator provides the Azure services used by the local
`StormWatch.Chat` application. Participants build application code; they do not
need an Azure subscription, portal access, resource role, or admin credential.

## What Runs Where

- `StormWatch.Chat` runs on the participant machine and maintains conversation
  history.
- The chat and embedding models run in a dedicated Microsoft Foundry resource.
- Approved `.md` and `.txt` knowledge files live in the participant's local
   `rag-data` folder.
- The chatbot stores generated vectors in process memory and searches them
   locally with cosine similarity.
- The StormWatch MCP server runs locally as a child process over stdio.
- OpenWeather, chat-model, and embedding-model requests use outbound HTTPS.
- No Azure service connects inbound to the participant machine.

The Foundry model used by `StormWatch.Chat` is separate from the model selected
inside GitHub Copilot Chat.

## Facilitator Resource Contract

Prepare these resources before the workshop:

1. A dedicated, disposable Foundry resource with a chat model deployment that
   supports tool calling.
2. An embedding model deployment in the same workshop resource.
3. A `rag-data` folder containing approved educational storm-preparedness
   `.md` and `.txt` files.
4. A temporary OpenWeather key with access to Direct Geocoding and the 5 Day /
   3 Hour Forecast APIs.

Use conservative model quota appropriate for startup corpus embedding and the
expected concurrent participants. Keep the Foundry resource free of
non-workshop deployments because its API key is resource-wide.

## Local Corpus Contract

Place the same approved corpus in each participant's `starter/rag-data` folder.
The application recursively reads only `.md` and `.txt` files. Keep the corpus
within these tracked limits:

| Limit | Value |
| --- | ---: |
| Supported files | 100 |
| Bytes per file | 512,000 |
| Total chunks | 500 |
| Chunk length | 1,200 characters |
| Chunk overlap | 200 characters |
| Retrieved passages | 3 |

The corpus should contain enough distinct material to test:

- one question with a clearly relevant passage;
- one question with multiple possible passages;
- one unsupported question; and
- recognizable filenames that participants can inspect in citations.

Source files and vectors remain local. The application sends every chunk to the
embedding deployment once at startup and sends each question once for query
embedding. Retrieved chunks are later sent to the chat model for grounded
generation. Use only public or approved educational content; never include
patient, personal, product, operational, or other restricted data.

## Participant Environment Files

Place these three private files in each participant's `starter/` folder.

### `.env`

```dotenv
FOUNDRY_MODEL_ENDPOINT=https://<dedicated-workshop-resource>.openai.azure.com/
FOUNDRY_MODEL_DEPLOYMENT=<deployment-name>
FOUNDRY_EMBEDDING_DEPLOYMENT=<embedding-deployment-name>
FOUNDRY_MODEL_API_KEY=<temporary-workshop-key>
```

### `.env.openweather`

```dotenv
OPENWEATHER_API_KEY=<temporary-workshop-key>
```

The repository ignores both populated files. Their tracked `.example`
counterparts contain placeholders only. Distribute the populated files and
`rag-data` through
an approved private channel; never paste their values into chat, commands,
screenshots, logs, or source.

This separation is intentional:

- model-only chat loads `.env` but does not call the embedding deployment;
- RAG mode uses the embedding deployment from `.env` and local `rag-data`; and
- the local MCP server receives only `.env.openweather` or a fixture path.

Participant-written MCP code therefore never receives Foundry credentials.

## Readiness Checks

From `starter/`, verify package restore and project compilation:

```powershell
dotnet build .\StormWatch.Chat\StormWatch.Chat.csproj
dotnet build .\stormwatch\StormWatch.csproj
```

Then perform these facilitator-owned live checks without displaying a key or
credential-bearing URL:

1. Send one harmless prompt through the configured chat deployment.
2. Embed one harmless sentence and confirm that the embedding deployment returns
   a non-empty numeric vector.
3. Confirm `rag-data` contains only approved `.md` and `.txt` files within the
   documented bounds.
4. Call OpenWeather Direct Geocoding and the forecast endpoint once.
5. Run the complete reference chatbot against one known-good and one unsupported
   corpus question; verify local file citations and bounded uncertainty.
6. Run the reference chatbot with fixture-backed MCP tools and confirm a model
   tool call produces the expected Bengaluru assessment.

Do not use participant, patient, product, operational, or other restricted data
for readiness checks. Corpus chunks and user questions are sent to the embedding
deployment; retrieved passages, questions, tool arguments, and tool results may
be sent to the chat model.

## Rotation and Cleanup

After the workshop:

1. rotate the OpenWeather key;
2. regenerate both Foundry resource keys;
3. remove distributed environment files according to local policy; and
4. delete disposable resources when they are no longer needed.
