namespace Mt.Api.Tenancy;

public sealed class TenantOptions {
    public required string HeaderName { get; set; } = "X-Tenant";
    public required Dictionary<string, TenantDefinition> Tenants { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class TenantDefinition {
    public required string Name { get; set; } = "";
}
