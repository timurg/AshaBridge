# AshaBridge

AshaBridge is a contract-first MCP platform on .NET.

Core executes contracts. Extensions provide methods. MCP exposes tools. ASP.NET Core hosts the platform.

## Projects

- `AshaBridge.Api` - thin ASP.NET Core host.
- `AshaBridge.AspNetCore` - DI, auth, health, diagnostics, MCP transport mapping.
- `AshaBridge.Core` - registries and streaming invocation runtime.
- `AshaBridge.Sdk` - public contracts, handlers, attributes, invocation events.
- `AshaBridge.PluginHost` - extension manifest and folder loading primitives.
- `AshaBridge.Caching` - cache key builder and memory single-flight cache.
- `AshaBridge.Persistence` - audit and idempotency abstractions.
- `AshaBridge.Extensions.Bitrix24` - official Bitrix24 extension.
- `AshaBridge.Extensions.Moodle` - official Moodle extension.

## Single-User Configuration

AshaBridge is configured as a single-user server. All important settings live in:

```text
src/AshaBridge.Api/appsettings.json
```

This is intentionally simpler than a multi-tenant token database. It is meant for a private server controlled by one user.

Do not commit real passwords, service tokens, Bitrix24 webhooks, or Moodle tokens to a public repository. For a private local/server deployment, edit `appsettings.json` directly on that machine.

## Main Settings

`security:user` is the human/admin login. It uses Basic Auth for protected REST endpoints:

```json
"user": {
  "username": "admin",
  "password": "change-me",
  "organizationId": "default",
  "permissions": [
    "bitrix.crm.item.read",
    "bitrix.crm.item.write",
    "bitrix.crm.deal.read",
    "bitrix.crm.contact.read",
    "bitrix.timeline.write",
    "moodle.user.read",
    "moodle.course.read",
    "moodle.progress.read",
    "moodle.grade.read"
  ]
}
```

`security:serviceTokens` is for clients such as n8n. These clients use `Authorization: Bearer <token>`:

```json
"serviceTokens": [
  {
    "name": "n8n",
    "token": "dev-token",
    "organizationId": "default",
    "permissions": [
      "bitrix.crm.item.read",
      "bitrix.crm.item.write",
      "bitrix.crm.deal.read",
      "bitrix.crm.contact.read",
      "bitrix.timeline.write",
      "moodle.user.read",
      "moodle.course.read",
      "moodle.progress.read",
      "moodle.grade.read"
    ]
  }
]
```

`bitrix` configures the Bitrix24 REST connection:

```json
"bitrix": {
  "defaultInstance": "office",
  "instances": {
    "office": {
      "baseUrl": "https://example.bitrix24.com",
      "authMode": "webhook",
      "webhookUrl": "https://example.bitrix24.com/rest/1/replace-with-webhook-token/",
      "timeoutSeconds": 20
    }
  }
}
```

`moodle` configures the Moodle REST connection:

```json
"moodle": {
  "defaultInstance": "main",
  "instances": {
    "main": {
      "baseUrl": "https://example.edu",
      "token": "replace-with-moodle-token",
      "timeoutSeconds": 20
    }
  }
}
```

## Broad n8n Permissions

For a trusted single-user n8n integration, grant the current full set of built-in permissions:

```json
[
  "bitrix.crm.item.read",
  "bitrix.crm.item.write",
  "bitrix.crm.deal.read",
  "bitrix.crm.contact.read",
  "bitrix.timeline.write",
  "moodle.user.read",
  "moodle.course.read",
  "moodle.progress.read",
  "moodle.grade.read"
]
```

These match the `RequiresPermission` attributes used by the current Bitrix24 and Moodle contracts.

## Install

```powershell
git clone <repository-url>
cd AshaBridge
dotnet restore src/AshaBridge.Api/AshaBridge.Api.csproj
dotnet build src/AshaBridge.Api/AshaBridge.Api.csproj
```

## Run

PowerShell:

```powershell
.\scripts\run.ps1
```

Bash:

```bash
./scripts/run.sh
```

Or run directly:

```powershell
dotnet run --project src/AshaBridge.Api/AshaBridge.Api.csproj --urls http://127.0.0.1:5088
```

## Endpoints

- `GET /` - application status.
- `GET /health` - health check.
- `GET /ready` - readiness check.
- `/mcp` - MCP transport, requires auth.
- `GET /internal/ashabridge/methods` - registered methods, requires auth.
- `GET /internal/ashabridge/extensions` - registered extensions, requires auth.
- `GET /internal/ashabridge/contracts` - registered contracts, requires auth.

Bearer token check:

```powershell
Invoke-RestMethod http://127.0.0.1:5088/internal/ashabridge/methods `
  -Headers @{ Authorization = "Bearer dev-token" }
```

Basic Auth check:

```powershell
$pair = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("admin:change-me"))
Invoke-RestMethod http://127.0.0.1:5088/internal/ashabridge/methods `
  -Headers @{ Authorization = "Basic $pair" }
```
