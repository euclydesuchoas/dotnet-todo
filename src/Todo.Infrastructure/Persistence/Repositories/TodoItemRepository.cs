using Microsoft.EntityFrameworkCore;
using Todo.Application.Abstractions.Persistence;
using Todo.Domain.TodoItems;

namespace Todo.Infrastructure.Persistence.Repositories;

internal sealed class TodoItemRepository(TodoDbContext dbContext) : ITodoItemRepository
{
    public Task<TodoItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.TodoItems
            .AsNoTracking()
            .FirstOrDefaultAsync(todoItem => todoItem.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TodoItem>> ListAsync(
        string? title,
        bool? isCompleted,
        DateTime? dueFrom,
        DateTime? dueTo,
        CancellationToken cancellationToken)
    {
        var query = dbContext.TodoItems.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(todoItem => todoItem.Title.Contains(title));
        }

        if (isCompleted.HasValue)
        {
            query = query.Where(todoItem => todoItem.IsCompleted == isCompleted.Value);
        }

        if (dueFrom.HasValue)
        {
            query = query.Where(todoItem => todoItem.DueDate >= dueFrom.Value);
        }

        if (dueTo.HasValue)
        {
            query = query.Where(todoItem => todoItem.DueDate <= dueTo.Value);
        }

        return await query
            .OrderBy(todoItem => todoItem.DueDate)
            .ToArrayAsync(cancellationToken);
    }

    public Task AddAsync(TodoItem todoItem, CancellationToken cancellationToken)
    {
        // Sem AddAsync do EF Core: ele só é necessário quando o banco gera o valor da
        // chave, e aqui o identificador já vem pronto do domínio.
        dbContext.TodoItems.Add(todoItem);

        return Task.CompletedTask;
    }
}
