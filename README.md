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
    "bitrix.crm.deal.write",
    "bitrix.crm.contact.read",
    "bitrix.crm.contact.write",
    "bitrix.crm.activity.write",
    "bitrix.user.read",
    "bitrix.timeline.write",
    "moodle.user.read",
    "moodle.user.write",
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
      "bitrix.crm.deal.write",
      "bitrix.crm.contact.read",
      "bitrix.crm.contact.write",
      "bitrix.crm.activity.write",
      "bitrix.user.read",
      "bitrix.timeline.write",
      "moodle.user.read",
      "moodle.user.write",
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
  "bitrix.crm.deal.write",
  "bitrix.crm.contact.read",
  "bitrix.crm.contact.write",
  "bitrix.crm.activity.write",
  "bitrix.user.read",
  "bitrix.timeline.write",
  "moodle.user.read",
  "moodle.user.write",
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

## Logging

Serilog writes to the console and to daily rolling files. The default file path is
`logs/ashabridge-.log` relative to the process working directory. Override it for
systemd with `AshaBridgeLogging__FilePath`:

```ini
Environment=AshaBridgeLogging__FilePath=/var/log/ashabridge/ashabridge-.log
```

Create the directory once and grant it to the service user:

```bash
sudo install -d -o <service-user> -g <service-group> /var/log/ashabridge
sudo systemctl daemon-reload
sudo systemctl restart ashabridge
```

If the configured directory is not writable, file logging is disabled and the
application continues logging to stdout/journald with a warning.

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

## Integration Tests

Integration tests make real HTTPS requests to Bitrix24 and Moodle using tokens from:

```text
src/AshaBridge.Api/appsettings.json
```

Run them with:

```powershell
dotnet test tests/AshaBridge.IntegrationTests/AshaBridge.IntegrationTests.csproj
```

The tests auto-discover Bitrix24 deals/contacts and Moodle user/course data where possible. The Moodle test user is looked up by email and created with a random password when missing. For exact records or write checks, fill `integrationTests` in `appsettings.json`.

Keep `integrationTests:allowWrites` set to `false` unless you intentionally want tests to call write methods such as `crm.item.update` and `crm.timeline.comment.add`.

The Moodle external service linked to the token must allow these functions:

- `core_user_get_users_by_field`
- `core_user_get_users`
- `core_user_create_users`
- `core_user_update_users`
- `core_auth_request_password_reset`
- `core_enrol_get_users_courses`
- `enrol_manual_enrol_users`
- `core_course_get_courses`
- `core_course_get_courses_by_field`
- `core_course_get_contents`
- `core_completion_get_activities_completion_status`
- `core_completion_get_course_completion_status`
- `core_competency_list_user_plans`
- `gradereport_user_get_grade_items`
