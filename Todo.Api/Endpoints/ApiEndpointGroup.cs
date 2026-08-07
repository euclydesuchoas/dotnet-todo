namespace Todo.Api.Endpoints;

/// <summary>
/// Grupo raiz da API. Comportamento aplicado aqui vale para todos os endpoints.
/// </summary>
public sealed class ApiEndpointGroup : IEndpointGroup
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent)
    {
        return parent.MapGroup(EndpointRoutes.Segments.Api);
    }
}
