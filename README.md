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
