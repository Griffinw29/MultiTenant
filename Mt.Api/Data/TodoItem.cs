namespace Mt.Api.Data;

public sealed class TodoItem : ITenantScoped
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TenantId { get; set; } = "";

    public string Title { get; set; } = "";
    public bool IsDone { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
