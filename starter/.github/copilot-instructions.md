# StormWatch Repository Instructions

- Target .NET 10 with nullable reference types enabled.
- Keep weather transport, storm-risk rules, CLI rendering, and MCP transport in
  separate C# types.
- Use the injected `HttpClient` and `System.Text.Json` for OpenWeather access.
- Use the official `ModelContextProtocol` C# SDK for server registration, stdio
  transport, tool discovery, and tool attributes.
- Keep the Foundry client separate from the MCP server. Authenticate model
  access with `DefaultAzureCredential`; never add or request a shared model key.
- Never place, print, log, return, or commit an OpenWeather API key. Read it only
  from `OPENWEATHER_API_KEY` or the masked VS Code MCP input.
- Unit tests must use synthetic fixtures or mocks and must never call a live API.
- Treat tests and the thresholds in the lab README as requirements. Do not alter
  them merely to make a failing implementation pass.
- Risk output must say it is an educational heuristic, not an official warning
  or reliable storm prediction.
- A stdio MCP server must not write application output to stdout because stdout
  carries protocol messages. Keep logging providers cleared in MCP mode.
- Keep tool responses concise, structured, deterministic, and free of secrets.
- Treat any frontend as a presentation adapter over the existing services. Keep
  OpenWeather credentials and requests server-side, and do not duplicate risk
  rules or parsing in browser code.