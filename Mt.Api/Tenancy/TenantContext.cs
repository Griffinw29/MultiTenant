namespace Mt.Api.Tenancy;

public sealed class TenantContext {
    public string TenantId { get; set; } = "";
    public string TenantName { get; set; } = "";
    public bool IsSet { get; private set; }

    public void SetTenant(string tenantId, string tenantName)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        IsSet = true;
    }
}
