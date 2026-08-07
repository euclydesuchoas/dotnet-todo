namespace Todo.Api.Endpoints;

/// <summary>
/// Um nó da árvore de rotas. É o ponto único onde se concentra o prefixo e todo
/// comportamento comum aos endpoints do grupo (tags, autorização, rate limiting,
/// CORS, filtros), via <see cref="RouteGroupBuilder"/>.
/// </summary>
/// <remarks>
/// Implementar esta interface diretamente cria um grupo na raiz da aplicação.
/// Para aninhar dentro de outro grupo, use <see cref="IEndpointGroup{TParent}"/>.
/// </remarks>
public interface IEndpointGroup
{
    RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent);
}

/// <summary>
/// Grupo aninhado em <typeparamref name="TParent"/>, herdando prefixo e metadados dele.
/// </summary>
public interface IEndpointGroup<TParent> : IEndpointGroup
    where TParent : IEndpointGroup;
