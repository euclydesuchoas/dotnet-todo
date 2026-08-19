using Todo.Api.Common.Http;
using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;
using Todo.Application.TodoItems;
using Todo.Application.TodoItems.GetTodoItems;

namespace Todo.Api.Endpoints.V1.TodoItems;

/// <summary>
/// Lista tarefas filtrando por título, conclusão e intervalo de vencimento.
/// </summary>
/// <remarks>
/// As datas são <see cref="DateTime"/> comuns, e chegam ao handler já em UTC: query string não
/// passa pelo <c>UtcDateTimeJsonConverter</c>, e quem cobre esse caminho é o
/// <see cref="UtcDateTimeEndpointFilter"/> aplicado no grupo raiz da API.
/// </remarks>
public sealed class GetTodoItemsEndpoint : IEndpoint<TodoItemEndpointGroup>
{
    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder group)
    {
        return group.MapGet("/", async (
            string? title,
            bool? isCompleted,
            DateTime? dueFrom,
            DateTime? dueTo,
            IServiceHandler<GetTodoItemsRequest, IReadOnlyList<TodoItemResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var request = new GetTodoItemsRequest(title, isCompleted, dueFrom, dueTo);

            var result = await handler.HandleAsync(request, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result.Base);
        })
        .WithName(EndpointNames.V1.GetTodoItems)
        .Produces<Result<IReadOnlyList<TodoItemResponse>>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest);
    }
}
