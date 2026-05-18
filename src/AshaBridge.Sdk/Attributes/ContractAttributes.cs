namespace AshaBridge.Sdk.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class McpMethodAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class ContractVersionAttribute(string version) : Attribute
{
    public string Version { get; } = version;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public sealed class RequiresPermissionAttribute(string permission) : Attribute
{
    public string Permission { get; } = permission;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class OperationRiskAttribute(OperationRisk risk) : Attribute
{
    public OperationRisk Risk { get; } = risk;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class CacheableAttribute : Attribute
{
    public int TtlSeconds { get; set; } = 60;

    public CacheScope Scope { get; set; } = CacheScope.Organization;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class RequiresIdempotencyAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class DoNotCacheAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
public sealed class InvalidatesCacheAttribute(string tagTemplate) : Attribute
{
    public string TagTemplate { get; } = tagTemplate;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false)]
public sealed class CacheKeyAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class McpDescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, Inherited = false)]
public sealed class McpParameterDescriptionAttribute(string description) : Attribute
{
    public string Description { get; } = description;
}

public enum OperationRisk
{
    Read,
    WriteLow,
    WriteMedium,
    WriteHigh,
    Admin
}

public enum CacheScope
{
    Global,
    Organization,
    Tenant,
    User
}
