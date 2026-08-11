using Microsoft.EntityFrameworkCore;
using Todo.Application.Abstractions.Persistence;
using Todo.Domain.Todos;

namespace Todo.Infrastructure.Persistence.Repositories;

internal sealed class TodoRepository(TodoDbContext dbContext) : ITodoRepository
{
    public Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Todos
            .AsNoTracking()
            .FirstOrDefaultAsync(todo => todo.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TodoItem>> ListAsync(
        string? title,
        bool? isCompleted,
        DateTime? dueFrom,
        DateTime? dueTo,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Todos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(todo => todo.Title.Contains(title));
        }

        if (isCompleted.HasValue)
        {
            query = query.Where(todo => todo.IsCompleted == isCompleted.Value);
        }

        if (dueFrom.HasValue)
        {
            query = query.Where(todo => todo.DueDate >= dueFrom.Value);
        }

        if (dueTo.HasValue)
        {
            query = query.Where(todo => todo.DueDate <= dueTo.Value);
        }

        return await query
            .OrderBy(todo => todo.DueDate)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddAsync(TodoItem todo, CancellationToken cancellationToken)
    {
        // Sem AddAsync do EF Core: ele só é necessário quando o banco gera o valor da
        // chave, e aqui o identificador já vem pronto do domínio.
        dbContext.Todos.Add(todo);

        return Task.CompletedTask;
    }
}
