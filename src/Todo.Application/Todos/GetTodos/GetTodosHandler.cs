using Todo.Application.Abstractions.Messaging;
using Todo.Application.Abstractions.Persistence;
using Todo.Application.Common.Results;

namespace Todo.Application.Todos.GetTodos;

internal sealed class GetTodosHandler(ITodoRepository todos)
    : IServiceHandler<GetTodosRequest, IReadOnlyList<TodoResponse>>
{
    public async Task<Result<IReadOnlyList<TodoResponse>>> HandleAsync(GetTodosRequest request, CancellationToken cancellationToken)
    {
        var found = await todos.ListAsync(
            request.Title,
            request.IsCompleted,
            request.DueFrom,
            request.DueTo,
            cancellationToken);

        var response = found
            .Select(todo => new TodoResponse
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                DueDate = todo.DueDate,
                IsCompleted = todo.IsCompleted,
            })
            .ToArray();

        return Result.Success<IReadOnlyList<TodoResponse>>(response);
    }
}
