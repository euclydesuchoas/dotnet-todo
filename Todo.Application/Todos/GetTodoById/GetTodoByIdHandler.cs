using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;

namespace Todo.Application.Todos.GetTodoById;

internal sealed class GetTodoByIdHandler : IServiceHandler<GetTodoByIdRequest, TodoResponse>
{
    public Task<Result<TodoResponse>> HandleAsync(GetTodoByIdRequest request, CancellationToken cancellationToken)
    {
        // TODO: Implement the logic to retrieve the Todo item from the store,
        // returning ResultError.NotFound when it does not exist.
        var response = new TodoResponse
        {
            Id = request.Id,
            Title = "Todo",
            Description = string.Empty,
            DueDate = DateTime.UtcNow,
            IsCompleted = false,
        };

        var result = Task.FromResult(Result.Success(response));
        return result;
    }
}
