using Todo.Api.Common.Http;

namespace Todo.Api.Endpoints;

/// <summary>
/// Grupo raiz da API. Comportamento aplicado aqui vale para todos os endpoints.
/// </summary>
public sealed class ApiEndpointGroup : IEndpointGroup
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent)
    {
        // A normalização de datas fica no grupo raiz porque é regra da API inteira: endpoint
        // novo já nasce com ela, sem depender de quem o escreve lembrar do fuso.
        return parent.MapGroup("/api")
            .AddEndpointFilter<UtcDateTimeEndpointFilter>();
    }
}
