using Microsoft.Extensions.Options;

namespace Mt.Api.Tenancy;

public sealed class TenantResolutionMiddleware {

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next) {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext, IOptions<TenantOptions> tenantOptions) {

        //method to allow requests to the swagger ui to pass through without resolving the tenant
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)) {
            await _next(context);
            return;
        }

        //get the tenant options
        var options = tenantOptions.Value;
        var headerName = options.HeaderName;
        var tenants = options.Tenants;
        if (tenants is null || tenants.Count == 0) {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new {
                error = "Tenancy configuration is missing or invalid",
                expectedSection = "Tenancy:Tenants"
            });
            return;
        }

        //check if the tenant id is present in the header
        if (!context.Request.Headers.TryGetValue(headerName, out var tenantId) || string.IsNullOrWhiteSpace(tenantId.ToString()) ) {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(
                new { error = "Tenant ID is required", 
                expectedHeader = headerName,
                example = $"{headerName}: acme" });
        return;
        }

        //get the tenant id
        tenantId = tenantId.ToString().Trim();

        //check if the tenant id is valid
        if (!tenants.TryGetValue(tenantId, out var tenant) || tenant is null || string.IsNullOrWhiteSpace(tenant.Name)) {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync( new { error = "Unknown Tenant"});
            return;
        }

        //set the tenant context
        tenantContext.SetTenant(tenantId, tenant.Name);

        //call the next middleware
        await _next(context);
    }
}
