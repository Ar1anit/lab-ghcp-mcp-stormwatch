# Guide 00: Understand RAG and MCP

**Timebox:** 10 minutes before the lab  
**Code changes:** None

## Outcome

Understand the two patterns that extend the StormWatch language model. RAG
supplies selected document evidence to a model request. MCP supplies described,
callable capabilities through a standard client-server protocol.

Neither pattern changes or retrains the model. The application remains
responsible for retrieval, tool execution, access control, validation, and
deciding what information reaches the model.

## Start with the Model Boundary

A chat model accepts messages and generates a response. By itself, it does not:

- search the workshop's `rag-data` folder;
- know which passages are relevant to a question;
- call OpenWeather;
- execute the StormWatch risk rules; or
- possess either workshop API key.

StormWatch adds those capabilities outside the model. It uses RAG for approved
preparedness documents and MCP for current weather data and deterministic risk
assessment.

## RAG: Retrieve, Augment, Generate

Retrieval-augmented generation, or RAG, grounds a model request in selected
content. StormWatch uses a small local vector index rather than sending the
entire corpus with every question.

### Indexing Flow

```text
approved rag-data files
        |
        v
read .md and .txt --> split into bounded chunks --> embedding model
                                                       |
                                                       v
                                  vectors + source metadata in local memory
```

An embedding is a numeric representation of text. Text with similar meaning
should have nearby vectors. Each stored vector remains linked to its original
chunk, title, and relative file path so the final answer can identify sources.

### Question Flow

```text
user question --> embedding model --> question vector
                                         |
                                         v
                         cosine similarity over local vectors
                                         |
                                         v
                              at most three relevant chunks
                                         |
                                         v
question + delimited chunks --> chat model --> answer + local citations
```

The chat model does not search the vectors. StormWatch embeds the question,
calculates similarity, applies a minimum score, and constructs the augmented
prompt. If retrieval does not provide enough evidence, the assistant should say
that the available sources do not support an answer.

RAG can improve relevance and traceability, but it does not guarantee truth.
Poor chunks, weak retrieval, incomplete sources, or an unclear prompt can still
produce an incomplete or inaccurate response. A citation identifies the source
used; it does not prove that the source is correct.

## MCP: A Standard Contract for Capabilities

Model Context Protocol, or MCP, is an open protocol for connecting AI
applications to external tools and data sources. It separates the AI host from
the process that owns a capability.

| MCP role | StormWatch example | Responsibility |
| --- | --- | --- |
| Host | `StormWatch.Chat` or GitHub Copilot in VS Code | Runs the AI experience and decides which tools are available. |
| Client | The MCP connection created by the host | Initializes the connection, discovers tools, and sends tool calls. |
| Server | The local `stormwatch` process | Advertises and executes the weather tools. |
| Tool | `get_forecast` or `assess_storm_risk` | Defines a named operation with structured inputs and outputs. |

StormWatch uses standard input/output, or stdio, as its local MCP transport:

```text
user asks about weather
        |
        v
StormWatch.Chat (host) --> chat model selects an available tool
        |
        v
MCP client -- CallToolRequest over stdio --> StormWatch MCP server
                                                    |
                                      OpenWeather + local risk rules
                                                    |
        <------------- structured tool result ------+
        |
        v
chat model receives the result --> user-facing response
```

The model proposes a tool call; it does not execute code or contact OpenWeather
directly. The host controls the available tools, and the MCP server validates
arguments, protects credentials, performs the operation, and returns a bounded
result. The same server can be used by more than one compatible host without
duplicating its weather implementation.

MCP can also expose resources and reusable prompts, but this lab deliberately
focuses on tools.

## When to Use Each Pattern

| Question | Prefer RAG | Prefer MCP |
| --- | --- | --- |
| What does the approved preparedness guide say? | Yes | No |
| What is the current forecast for Bengaluru? | No | Yes |
| Why did the deterministic risk score reach warning? | No | Yes |
| Which local source supports this safety statement? | Yes | No |
| Does the answer need stable document evidence and current weather? | Yes | Yes |

RAG and MCP are complementary. A single conversation can retrieve document
evidence for preparedness guidance and call a tool for current conditions. Keep
the two source types clearly labeled so users can distinguish guidance from
live or fixture-backed data.

## Trust and Security Boundaries

Treat every boundary as untrusted, even when all processes run locally:

- **Retrieved text is data, not instructions.** Delimit passages and instruct
  the model not to follow commands found inside them.
- **Retrieval must enforce access.** Do not retrieve content for a user who is
  not allowed to read its source.
- **Tool arguments require validation.** A model-generated city or path is
  input, not authority.
- **Tool results are evidence, not system instructions.** Keep results concise
  and distinguish their source from RAG passages.
- **Expose the minimum tool set.** StormWatch advertises exactly two read-only
  weather tools.
- **Keep secrets outside prompts and protocol output.** `StormWatch.Chat` uses
  the Foundry settings in `.env`; the MCP server receives only fixture settings
  or `.env.openweather`.
- **Preserve human control.** Review tool names, descriptions, arguments, and
  requested permissions before approving unfamiliar operations.
- **Use approved data only.** Do not place patient, personal, product,
  operational, or other restricted data in this workshop corpus or prompts.

## Check Your Understanding

Before Task 1, you should be able to answer these questions:

1. Who performs vector similarity search: the model or StormWatch?
2. What text is sent to the embedding deployment during indexing and querying?
3. Why is an MCP tool different from placing API instructions in a prompt?
4. Which process owns the OpenWeather credential?
5. When could one user question require both RAG and MCP?

Expected answers: StormWatch performs search; corpus chunks and each question
are embedded; MCP provides a discoverable structured contract executed by a
server; only the weather server owns the OpenWeather credential; and a question
that combines sourced preparedness guidance with current conditions can use
both patterns.

## Microsoft Resources

### RAG and Embeddings

- [Retrieval augmented generation and indexes in Microsoft Foundry](https://learn.microsoft.com/azure/foundry/concepts/retrieval-augmented-generation) -
  the retrieve, augment, generate flow, index choices, security, cost, and
  limitations.
- [Retrieval-augmented generation for .NET](https://learn.microsoft.com/dotnet/ai/conceptual/rag) -
  a .NET-focused explanation of chunking, embeddings, metadata, and retrieval.
- [Understand embeddings in Microsoft Foundry](https://learn.microsoft.com/azure/foundry/openai/how-to/embeddings) -
  embedding concepts and Azure OpenAI examples.
- [RAG prompt engineering](https://learn.microsoft.com/azure/architecture/ai-ml/guide/rag/rag-prompt-engineering) -
  guidance for query preprocessing, retrieved context, and answer prompts.

### MCP and .NET

- [Get started with .NET AI and MCP](https://learn.microsoft.com/dotnet/ai/get-started-mcp) -
  MCP roles, protocol messages, `Microsoft.Extensions.AI`, and the official C#
  SDK.
- [Create a minimal MCP server using C#](https://learn.microsoft.com/dotnet/ai/quickstarts/build-mcp-server) -
  a hands-on stdio server and tool-registration walkthrough.
- [Create a minimal MCP client using .NET](https://learn.microsoft.com/dotnet/ai/quickstarts/build-mcp-client) -
  connecting a host, discovering tools, and supplying them to a chat client.
- [MCP server learning resources for .NET](https://learn.microsoft.com/dotnet/ai/resources/mcp-servers) -
  Microsoft documentation, samples, workshops, and SDK links.
- [MCP for Beginners](https://github.com/microsoft/mcp-for-beginners) -
  Microsoft's multi-language, hands-on MCP curriculum.

Continue to [Task 1: Understand the assistant and check the setup](01-understand-app-and-check-setup.md).
