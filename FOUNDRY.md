# Microsoft Foundry Model Setup

StormWatch includes a complete local client in `StormWatch.FoundryClient`. The
client uses a model deployment managed in Microsoft Foundry and gives that model
the tools discovered from the participant's local StormWatch MCP server.

## What Runs Where

- The model runs in Microsoft Foundry.
- `StormWatch.FoundryClient` runs on the participant machine.
- The StormWatch MCP server runs as a child process on the participant machine.
- MCP traffic uses local stdio; Azure does not connect inbound to the laptop.
- Model requests use outbound HTTPS and Microsoft Entra ID authentication.

This is separate from GitHub Copilot Chat. GitHub Copilot can test the same MCP
server, but its model picker is not configured by this Foundry deployment.

## Participant Readiness

The facilitator supplies the endpoint and deployment name. These values are not
secrets, but access to the deployment is controlled through Azure RBAC.

From `starter/`:

```powershell
az login --tenant <tenant-id>
az account show

$env:FOUNDRY_MODEL_ENDPOINT = "https://<resource-name>.openai.azure.com/"
$env:FOUNDRY_MODEL_DEPLOYMENT = "<deployment-name>"

dotnet build .\StormWatch.FoundryClient\StormWatch.FoundryClient.csproj
```

Do not put endpoint configuration or credentials in committed source. The
client uses `DefaultAzureCredential`, which can use the participant's Azure CLI
login during the workshop.

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

Confirm that Azure CLI is signed into the tenant containing the Foundry resource
and that the signed-in participant has data-plane permission to invoke the
model. RBAC changes can take several minutes to propagate.

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
