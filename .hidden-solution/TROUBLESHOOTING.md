# Facilitator Troubleshooting

Use the public [troubleshooting guide](../TROUBLESHOOTING.md) first. These notes
cover facilitator-owned service and recovery checks.

## Foundry

- Confirm the deployment supports chat and tool calling.
- Confirm the embedding deployment returns a non-empty vector and belongs to
  the same dedicated workshop endpoint.
- Confirm endpoint and deployment belong to the same dedicated resource.
- Inspect quota before asking participants to retry.
- If model access is unavailable, continue deterministic local-RAG and MCP
  implementation, but do not mark live chat complete.

## Local RAG

- Verify every participant receives the same approved `rag-data` directory.
- Confirm files are `.md` or `.txt` and remain within the documented corpus
  limits.
- Confirm the known-good question ranks a useful local passage and the
  unsupported question falls below the similarity threshold.
- Remember that source files and vectors stay local, but chunks and questions
  are sent to the embedding deployment.

## OpenWeather

- Validate Direct Geocoding before the forecast endpoint.
- Monitor account-level rate limits because usage is shared across participants.
- Use the fixture for acceptance whenever live responses are unreliable.

## MCP

- Run the reference protocol smoke before delivery.
- Confirm the child server receives only fixture configuration or
  `.env.openweather`.
- Restart VS Code tool discovery after schema changes.
- Never enable application logging on MCP stdout.

## Recovery

Restore only the current task's reference files, then rerun its focused tests.
Do not copy the entire reference tree unless the participant needs the final
integration experience and time is nearly exhausted.

If a credential appears in any visible artifact, rotate or revoke it. Removing
the artifact is not sufficient after exposure.
