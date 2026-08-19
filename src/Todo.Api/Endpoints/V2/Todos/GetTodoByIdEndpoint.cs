using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;
using Todo.Application.Todos;
using Todo.Application.Todos.GetTodoById;

namespace Todo.Api.Endpoints.V2.Todos;

public sealed class GetTodoByIdEndpoint : IEndpoint<TodoEndpointGroup>
{
    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder group)
    {
        return group.MapGet("/{id:guid}", async (Guid id, IServiceHandler<GetTodoByIdRequest, TodoResponse> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetTodoByIdRequest(id), cancellationToken);

            if (result.IsSuccess)
            {
                return Results.Ok(result);
            }

            return result.Error.Type is ResultErrorTypeEnum.NotFound
                ? Results.NotFound(result.Base)
                : Results.BadRequest(result.Base);
        })
        .WithName(EndpointNames.V2.GetTodoById)
        .Produces<Result<TodoResponse>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest)
        .Produces<Result>(StatusCodes.Status404NotFound);
    }
}
