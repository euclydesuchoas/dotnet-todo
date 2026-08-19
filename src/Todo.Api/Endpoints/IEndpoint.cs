namespace Todo.Api.Endpoints;

/// <summary>
/// Contrato de mapeamento usado pelo registrador. Não implemente esta interface
/// diretamente: use <see cref="IEndpoint{TGroup}"/> para declarar a qual grupo o
/// endpoint pertence.
/// </summary>
public interface IEndpoint
{
    RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder group);
}

/// <summary>
/// Endpoint pertencente a <typeparamref name="TGroup"/>. O <c>IEndpointRouteBuilder</c>
/// recebido em <see cref="IEndpoint.MapEndpoint"/> já é o grupo configurado, então a rota
/// mapeada aqui é relativa a ele e herda seus metadados.
/// </summary>
public interface IEndpoint<TGroup> : IEndpoint
    where TGroup : IEndpointGroup;
