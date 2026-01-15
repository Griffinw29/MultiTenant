using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Mt.Api.Tenancy;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Mt.Api.Swagger;

public sealed class TenantHeaderOperationFilter : IOperationFilter
{
    private readonly string _headerName;

    public TenantHeaderOperationFilter(IOptions<TenantOptions> options)
    {
        _headerName = options.Value.HeaderName;
        if (string.IsNullOrWhiteSpace(_headerName))
        {
            _headerName = "X-Tenant";
        }
    }

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null)
        {
            operation.Parameters = new List<IOpenApiParameter>();
        }

        var alreadyDefined = operation.Parameters.Any(p =>
            p.In == ParameterLocation.Header &&
            string.Equals(p.Name, _headerName, StringComparison.OrdinalIgnoreCase));

        if (alreadyDefined)
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = _headerName,
            In = ParameterLocation.Header,
            Required = true,
            Description = "Tenant id, e.g. acme",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });
    }
}
