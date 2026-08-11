using Todo.Application.Abstractions.Messaging;
using Todo.Domain.Common;

namespace Todo.Application.Todos.GetTodos;

/// <summary>
/// Filtros da listagem. Todos opcionais; ausentes não restringem.
/// </summary>
/// <remarks>
/// As datas são <see cref="UtcDateTime"/>, e não <see cref="DateTime"/>, porque este request
/// é vinculado de query string. Ali não há <c>System.Text.Json</c> nem
/// <c>UtcDateTimeJsonConverter</c>: o vínculo chama o <c>TryParse</c> do tipo declarado, então
/// é o tipo que decide se o valor chega normalizado.
/// </remarks>
public sealed record GetTodosRequest(
    string? Title = null,
    bool? IsCompleted = null,
    UtcDateTime? DueFrom = null,
    UtcDateTime? DueTo = null
) : IRequest<IReadOnlyList<TodoResponse>>;
