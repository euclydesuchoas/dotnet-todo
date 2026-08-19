using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;
using Todo.Application.TodoItems;
using Todo.Application.TodoItems.GetTodoItemById;

namespace Todo.Api.Endpoints.V1.TodoItems;

public sealed class GetTodoItemByIdEndpoint : IEndpoint<TodoItemEndpointGroup>
{
    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder group)
    {
        return group.MapGet("/{id:guid}", async (Guid id, IServiceHandler<GetTodoItemByIdRequest, TodoItemResponse> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetTodoItemByIdRequest(id), cancellationToken);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return result.Error.Type is ResultErrorTypeEnum.NotFound
                ? Results.NotFound(result.Base)
                : Results.BadRequest(result.Base);
        })
        .WithName(EndpointNames.V1.GetTodoItemById)
        .Produces<Result<TodoItemResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces<Result>(StatusCodes.Status404NotFound);
    }
}
