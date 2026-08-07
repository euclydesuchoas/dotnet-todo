namespace Todo.Api.Endpoints.V1;

/// <summary>
/// Raiz da v1. O <c>WithGroupName</c> é o que associa todos os endpoints
/// descendentes ao documento OpenAPI <c>v1</c>.
/// </summary>
public sealed class V1EndpointGroup : IEndpointGroup<ApiEndpointGroup>
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent)
    {
        return parent.MapGroup($"/{ApiVersions.V1}")
            .WithGroupName(ApiVersions.V1);
    }
}
