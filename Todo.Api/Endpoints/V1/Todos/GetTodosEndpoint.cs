using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;
using Todo.Application.Todos;
using Todo.Application.Todos.GetTodos;
using Todo.Domain.Common;

namespace Todo.Api.Endpoints.V1.Todos;

/// <summary>
/// Lista tarefas filtrando por título, conclusão e intervalo de vencimento.
/// </summary>
/// <remarks>
/// Os parâmetros de data são <see cref="UtcDateTime"/> de propósito. Query string não passa
/// pelo <c>UtcDateTimeJsonConverter</c> — o vínculo do minimal API chama o <c>TryParse</c> do
/// tipo declarado. Com <see cref="DateTime"/> ali, <c>?dueFrom=2027-03-10T09:00:00Z</c>
/// chegaria como <see cref="DateTimeKind.Local"/> já convertido, e o filtro compararia hora
/// local com coluna em UTC.
/// </remarks>
public sealed class GetTodosEndpoint : IEndpoint<TodoEndpointGroup>
{
    public RouteHandlerBuilder MapEndpoint(IEndpointRouteBuilder group)
    {
        return group.MapGet("/", async (
            string? title,
            bool? isCompleted,
            UtcDateTime? dueFrom,
            UtcDateTime? dueTo,
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
