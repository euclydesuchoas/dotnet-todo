using Todo.Domain.TodoItems;

namespace Todo.Application.Abstractions.Persistence;

public interface ITodoItemRepository
{
    Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Tarefas que satisfazem todos os filtros informados. Filtro nulo não restringe.
    /// </summary>
    /// <remarks>
    /// As datas chegam aqui já em UTC — quem normaliza é a borda que as recebeu. O intervalo
    /// é fechado dos dois lados.
    /// </remarks>
    Task<IReadOnlyList<TodoItem>> ListAsync(
        string? title,
        bool? isCompleted,
        DateTime? dueFrom,
        DateTime? dueTo,
        CancellationToken cancellationToken);

    Task AddAsync(TodoItem todoItem, CancellationToken cancellationToken);
}
