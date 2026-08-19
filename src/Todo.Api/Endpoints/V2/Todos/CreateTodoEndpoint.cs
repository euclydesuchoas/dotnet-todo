using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;
using Todo.Application.Todos.CreateTodo;

namespace Todo.Api.Endpoints.V2.Todos;

public sealed class CreateTodoEndpoint : IEndpoint<TodoEndpointGroup>
{
    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder group)
    {
        return group.MapPost("/", async (CreateTodoRequest request, IServiceHandler<CreateTodoRequest, Guid> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);

            // Diferença em relação à v1: devolve o header Location apontando para o recurso criado.
            // O caminho é gerado a partir da rota do GET by id, e não montado à mão.
            return result.IsSuccess
                ? Results.CreatedAtRoute(EndpointNames.V2.GetTodoById, new { id = result.Data }, result)
                : Results.BadRequest(result.Base);
        })
        .WithName(EndpointNames.V2.CreateTodo)
        .Produces<Result<Guid>>(StatusCodes.Status201Created)
        .Produces<Result>(StatusCodes.Status400BadRequest);
    }
}
