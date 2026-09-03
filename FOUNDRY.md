# Microsoft Foundry Model Setup

StormWatch includes a complete local client in `StormWatch.FoundryClient`. The
client uses a model deployment managed in Microsoft Foundry and gives that model
the tools discovered from the participant's local StormWatch MCP server.

## What Runs Where

- The model runs in Microsoft Foundry.
- `StormWatch.FoundryClient` runs on the participant machine.
- The StormWatch MCP server runs as a child process on the participant machine.
- MCP traffic uses local stdio; Azure does not connect inbound to the laptop.
- Model requests use outbound HTTPS and a temporary workshop API key.

This is separate from GitHub Copilot Chat. GitHub Copilot can test the same MCP
server, but its model picker is not configured by this Foundry deployment.

## Participant Readiness

The facilitator privately supplies a `.env` file for a dedicated workshop
resource. Participants do not need Azure credentials, an Azure role, or access
to the Foundry portal.

From `starter/`:

```powershell
dotnet build .\StormWatch.FoundryClient\StormWatch.FoundryClient.csproj
```

Place the facilitator-supplied `.env` in the `starter/` directory. It contains:

```dotenv
FOUNDRY_MODEL_ENDPOINT=https://<dedicated-workshop-resource>.openai.azure.com/
FOUNDRY_MODEL_DEPLOYMENT=<deployment-name>
FOUNDRY_MODEL_API_KEY=<temporary-workshop-key>
```

The repository ignores `.env`. Never commit it, paste its key into Copilot
Chat, include it in screenshots, or print it during troubleshooting. Process
environment variables override matching `.env` entries.

## End-To-End Local Test

Run this after both StormWatch MCP tool methods are complete:

```powershell
dotnet run --project .\StormWatch.FoundryClient -- `
  .\StormWatch\StormWatch.csproj `
  .\tests\fixtures\forecast.json `
  Bengaluru
```

The client must:

1. start the local MCP server in fixture mode;
2. discover exactly `get_forecast` and `assess_storm_risk`;
3. provide those tools to the Foundry model;
4. let the model select and invoke the appropriate local tool; and
5. return an answer containing the fixture's peak, score, level, evidence, and
   educational-use qualification.

The deterministic fixture keeps OpenWeather credentials out of this validation
path. Conversation content and selected tool results are sent to the Foundry
model deployment, so do not substitute customer, patient, operational, or other
restricted data.

## Common Failures

### Authentication failure

Confirm that `.env` is in the directory from which the command is running and
contains all three required names. Ask the facilitator for a replacement file
if the key has expired or been rotated; do not send the key through chat or
include it in diagnostic output.

### Deployment not found

Use the deployment name, not only the underlying model family name. Confirm that
the endpoint belongs to the resource containing that deployment.

### Model answers without a tool call

The expected fixture values must come from a StormWatch tool. Inspect the client
output and Foundry telemetry available to the facilitator; do not treat an
ungrounded answer as a passing result.

### MCP tools are missing

Build StormWatch first and verify that both SDK-attributed methods are complete.
The Foundry client stops before calling the model unless it discovers exactly
the two expected tools.

### Quota or rate limiting

The facilitator should size deployment capacity for concurrent participants and
stagger the final validation if needed. Repeated retries can increase load and
cost; use the direct SDK smoke test when model capacity is temporarily
unavailable.

## Facilitator Security Boundary

An API key authorizes inference across the resource, not only one named model
deployment. Use a disposable resource containing only workshop deployments,
apply conservative quota and rate limits, distribute `.env` through an approved
private channel, and regenerate both resource keys immediately after the
workshop. Delete the resource when it is no longer needed.
