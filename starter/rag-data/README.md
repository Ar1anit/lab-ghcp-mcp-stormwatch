# StormWatch RAG Corpus

This folder contains a synthetic knowledge corpus for an optional retrieval-
augmented generation exercise. The documents cover the complete StormWatch use
case: purpose, architecture, scoring rules, weather fields, MCP contracts,
chatbot behavior, scenarios, limitations, and terminology.

All names, forecasts, and scenarios are synthetic workshop data. The content is
safe to send to the workshop model deployment, but it is not operational
weather or safety guidance.

## Directory Layout

| Path | Purpose | Ingest into the index? |
| --- | --- | --- |
| `pdfs/*.pdf` | Text-searchable participant corpus | Yes |
| `source/*.md` | Canonical authoring source for review | No |
| `corpus-manifest.json` | Document metadata and intended topics | Optional |
| `rag-evaluation.jsonl` | Ground-truth retrieval and answer checks | No |

Ingest only the six files in `pdfs/` for the core exercise. Indexing both the
PDFs and their Markdown sources creates duplicate chunks and can distort
retrieval scores.

## Corpus Design

| Document ID | Document | Primary retrieval purpose |
| --- | --- | --- |
| `SW-001` | System Overview and Architecture | Purpose, actors, runtime flow, trust boundaries, RAG and MCP routing |
| `SW-002` | Risk Scoring and Interpretation | Exact thresholds, level bands, tie-breaking, worked examples |
| `SW-003` | Weather Data and Tool Contracts | Data fields, units, source behavior, MCP request and response contracts |
| `SW-004` | Chatbot Operations Playbook | Query routing, response pattern, failures, escalation, prompt-injection handling |
| `SW-005` | Synthetic City Scenarios | Grounded examples, boundary cases, source-conflict exercises |
| `SW-006` | FAQ, Glossary, and Responsible Use | Common questions, definitions, limitations, safe-use policy |

## Suggested Index Fields

The exact schema depends on the search service, but each chunk should retain:

| Field | Example | Purpose |
| --- | --- | --- |
| `chunk_id` | `SW-002-p03-c02` | Unique retrievable key |
| `document_id` | `SW-002` | Stable document identity |
| `title` | `Risk Scoring and Interpretation` | Citation and filtering |
| `category` | `risk-policy` | Metadata filter |
| `source_file` | `stormwatch-risk-scoring-reference.pdf` | User-visible citation |
| `page_number` | `3` | Citation precision |
| `section` | `Tiered thresholds` | Retrieval context |
| `content` | Extracted text | Keyword and semantic search |
| `content_vector` | Embedding vector | Vector similarity |
| `version` | `1.0` | Freshness control |
| `synthetic` | `true` | Safety and provenance |
| `security_classification` | `workshop-public` | Access filtering |

Use heading-aware chunks of roughly 700 to 900 tokens with 100 to 150 tokens of
overlap as a starting point. Keep tables with their heading and column labels.
Do not split a scoring rule from its threshold or a scenario from its expected
result.

## Recommended Retrieval Flow

1. Extract text and page numbers from `pdfs/*.pdf`.
2. Split text by heading-aware boundaries.
3. Store searchable text, embeddings, and the metadata above.
4. Use hybrid retrieval when available: keyword matching preserves exact values
   such as `990 hPa`, while vectors help with paraphrases.
5. Return the top three to five chunks and require file and page citations.
6. Route dynamic city questions to MCP instead of answering from scenario text.
7. Run `rag-evaluation.jsonl` and record both retrieval and answer quality.

The chatbot should follow this source order:

1. MCP tool output for a requested city's current or configured fixture result.
2. Retrieved corpus content for stable rules, explanations, architecture, and
   safe-use guidance.
3. General model knowledge only for conversational glue, never for a forecast,
   score, threshold, or safety claim.

## Minimal Chatbot Grounding Instruction

```text
Use retrieved StormWatch documents for stable reference knowledge. Use the
StormWatch MCP tools for city-specific forecast and assessment facts. Never
treat a synthetic scenario as current weather. Cite the retrieved PDF filename
and page. Label the score as an educational heuristic, not an official warning.
If the user asks for emergency or operational safety advice, direct them to
official local weather and emergency services.
```

## Evaluation

`rag-evaluation.jsonl` contains one JSON object per line. Important fields are:

- `retrieval_targets`: documents expected in the top retrieved results;
- `answer_contains`: facts expected in a grounded answer;
- `requires_tool`: whether document retrieval alone is insufficient;
- `expected_tool`: the MCP tool that should provide dynamic facts; and
- `behavior`: expected routing such as `rag_only`, `hybrid`, or
  `redirect_to_official_source`.

A useful workshop success gate is:

- the first expected document appears in the top three results for at least
  85 percent of `rag_only` questions;
- exact numeric thresholds are reproduced without invention;
- all `requires_tool` cases call or explicitly request the correct tool;
- scenario values are never described as current conditions; and
- every risk answer includes the educational-use qualification.

## Important Boundary

RAG adds stable knowledge and citations. MCP supplies dynamic, structured
forecast and assessment data. Neither source makes StormWatch a meteorological
forecast, emergency alert, medical device, or safety system.
