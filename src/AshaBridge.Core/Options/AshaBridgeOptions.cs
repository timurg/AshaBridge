using System.ComponentModel.DataAnnotations;

namespace AshaBridge.Core.Options;

public sealed class AshaBridgeOptions
{
    public AshaBridgeMcpOptions Mcp { get; set; } = new();

    public AshaBridgeExtensionsOptions Extensions { get; set; } = new();

    public AshaBridgeCacheOptions Cache { get; set; } = new();

    public AshaBridgeAuditOptions Audit { get; set; } = new();
}

public sealed class AshaBridgeMcpOptions
{
    [Required]
    public string ServerName { get; set; } = "AshaBridge MCP Platform";
}

public sealed class AshaBridgeExtensionsOptions
{
    public string Path { get; set; } = "./extensions";

    public List<string> Enabled { get; set; } = [];
}

public sealed class AshaBridgeCacheOptions
{
    public string Provider { get; set; } = "memory";

    public int DefaultTtlSeconds { get; set; } = 60;
}

public sealed class AshaBridgeAuditOptions
{
    public bool Enabled { get; set; } = true;
}
