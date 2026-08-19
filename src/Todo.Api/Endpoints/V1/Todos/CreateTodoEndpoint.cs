using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;
using Todo.Application.Todos.CreateTodo;

namespace Todo.Api.Endpoints.V1.Todos;

public sealed class CreateTodoEndpoint : IEndpoint<TodoEndpointGroup>
{
    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder group)
    {
        return group.MapPost("/", async (CreateTodoRequest request, IServiceHandler<CreateTodoRequest, Guid> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);

            return result.IsSuccess
                ? Results.Created((string?)null, result)
                : Results.BadRequest(result.Base);
        })
        .WithName(EndpointNames.V1.CreateTodo)
        .Produces<Result<Guid>>(StatusCodes.Status201Created)
        .Produces<Result>(StatusCodes.Status400BadRequest);
    }
}
