using Todo.Api.Common.Http;
using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;
using Todo.Application.Todos;
using Todo.Application.Todos.GetTodos;

namespace Todo.Api.Endpoints.V1.Todos;

/// <summary>
/// Lista tarefas filtrando por título, conclusão e intervalo de vencimento.
/// </summary>
/// <remarks>
/// As datas são <see cref="DateTime"/> comuns, e chegam ao handler já em UTC: query string não
/// passa pelo <c>UtcDateTimeJsonConverter</c>, e quem cobre esse caminho é o
/// <see cref="UtcDateTimeEndpointFilter"/> aplicado no grupo raiz da API.
/// </remarks>
public sealed class GetTodosEndpoint : IEndpoint<TodoEndpointGroup>
{
    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder group)
    {
        return group.MapGet("/", async (
            string? title,
            bool? isCompleted,
            DateTime? dueFrom,
            DateTime? dueTo,
            IServiceHandler<GetTodosRequest, IReadOnlyList<TodoResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var request = new GetTodosRequest(title, isCompleted, dueFrom, dueTo);

            var result = await handler.HandleAsync(request, cancellationToken);

            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result.Base);
        })
        .WithName(EndpointNames.V1.GetTodos)
        .Produces<Result<IReadOnlyList<TodoResponse>>>(StatusCodes.Status200OK)
        .Produces<Result>(StatusCodes.Status400BadRequest);
    }
}
