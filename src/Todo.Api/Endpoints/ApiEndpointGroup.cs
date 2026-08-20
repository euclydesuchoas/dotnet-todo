using Todo.Api.Common.Http;

namespace Todo.Api.Endpoints;

/// <summary>
/// Grupo raiz da API. Comportamento aplicado aqui vale para todos os endpoints.
/// </summary>
public sealed class ApiEndpointGroup : IEndpointGroup
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent)
    {
        // A normalização de datas e de texto fica no grupo raiz porque é regra da API inteira:
        // endpoint novo já nasce com ela, sem depender de quem o escreve lembrar do fuso nem da
        // forma Unicode.
        return parent.MapGroup("/api")
            .AddEndpointFilter<UtcDateTimeEndpointFilter>()
            .AddEndpointFilter<TextValueEndpointFilter>();
    }
}
