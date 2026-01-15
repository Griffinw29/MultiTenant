using Microsoft.EntityFrameworkCore;
using Mt.Api.Data;
using Mt.Api.Tenancy;
using Mt.Api;
using Mt.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<TenantHeaderOperationFilter>();
});

// add tenancy options and a scoped instance of the tenant context
builder.Services.Configure<TenantOptions>(builder.Configuration.GetSection("Tenancy"));
builder.Services.AddScoped<TenantContext>();

// Data
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var cs = builder.Configuration.GetConnectionString("AppDb");
    opt.UseSqlite(cs);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Ensure local dev database exists.
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();

// Must run BEFORE endpoints.
app.UseMiddleware<TenantResolutionMiddleware>();

// Tiny probe endpoint
app.MapGet("/whoami", (TenantContext tenant) => Results.Ok(new
{
    tenantId = tenant.TenantId,
    tenantName = tenant.TenantName
}));

app.MapToDoEndpoints();
app.Run();

// Needed for WebApplicationFactory in tests
public partial class Program { }
