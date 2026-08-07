namespace Todo.Api.Endpoints.V2;

/// <summary>
/// Raiz da v2. O <c>WithGroupName</c> é o que associa todos os endpoints
/// descendentes ao documento OpenAPI <c>v2</c>.
/// </summary>
public sealed class V2EndpointGroup : IEndpointGroup<ApiEndpointGroup>
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent)
    {
        return parent.MapGroup($"/{ApiVersions.V2}")
            .WithGroupName(ApiVersions.V2);
    }
}
