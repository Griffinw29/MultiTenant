namespace Mt.Api.Data;

public interface ITenantScoped
{
    string TenantId { get; set; }
}
