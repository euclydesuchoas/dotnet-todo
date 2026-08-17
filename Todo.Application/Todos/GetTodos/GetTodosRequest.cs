using Todo.Application.Abstractions.Messaging;

namespace Todo.Application.Todos.GetTodos;

/// <summary>
/// Filtros da listagem. Todos opcionais; ausentes não restringem.
/// </summary>
/// <remarks>
/// As datas chegam aqui já em UTC. Este request é vinculado de query string, onde não há
/// <c>System.Text.Json</c> para normalizar — quem cobre esse caminho é o filtro de endpoint
/// aplicado no grupo raiz da API, antes de o handler ser chamado.
/// </remarks>
public sealed record GetTodosRequest(
    string? Title = null,
    bool? IsCompleted = null,
    DateTime? DueFrom = null,
    DateTime? DueTo = null
) : IRequest<IReadOnlyList<TodoResponse>>;
