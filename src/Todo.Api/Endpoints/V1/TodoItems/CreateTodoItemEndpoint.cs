using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;
using Todo.Application.TodoItems.CreateTodoItem;

namespace Todo.Api.Endpoints.V1.TodoItems;

public sealed class CreateTodoItemEndpoint : IEndpoint<TodoItemEndpointGroup>
{
    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder group)
    {
        return group.MapPost("/", async (CreateTodoItemRequest request, IServiceHandler<CreateTodoItemRequest, Guid> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);

            return result.IsSuccess
                ? Results.Created((string?)null, result)
                : Results.BadRequest(result.Base);
        })
        .WithName(EndpointNames.V1.CreateTodoItem)
        .Produces<Result<Guid>>(StatusCodes.Status201Created)
        .Produces<Result>(StatusCodes.Status400BadRequest);
    }
}
