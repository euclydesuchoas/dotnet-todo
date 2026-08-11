using Todo.Application.Abstractions.Messaging;
using Todo.Application.Abstractions.Persistence;
using Todo.Application.Common.Results;

namespace Todo.Application.Todos.GetTodoById;

internal sealed class GetTodoByIdHandler(ITodoRepository todos)
    : IServiceHandler<GetTodoByIdRequest, TodoResponse>
{
    public async Task<Result<TodoResponse>> HandleAsync(GetTodoByIdRequest request, CancellationToken cancellationToken)
    {
        var todo = await todos.GetByIdAsync(request.Id, cancellationToken);

        if (todo is null)
        {
            return Result.Failure<TodoResponse>(
                ResultError.NotFound(ResultErrorCodes.NotFound, $"Todo '{request.Id}' was not found."));
        }

        var response = new TodoResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            DueDate = todo.DueDate,
            IsCompleted = todo.IsCompleted,
        };

        return Result.Success(response);
    }
}
