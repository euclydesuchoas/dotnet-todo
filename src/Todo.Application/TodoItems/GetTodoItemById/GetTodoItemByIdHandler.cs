using Todo.Application.Abstractions.Messaging;
using Todo.Application.Abstractions.Persistence;
using Todo.Application.Common.Results;

namespace Todo.Application.TodoItems.GetTodoItemById;

internal sealed class GetTodoItemByIdHandler(ITodoItemRepository todoItems)
    : IServiceHandler<GetTodoItemByIdRequest, TodoItemResponse>
{
    public async Task<Result<TodoItemResponse>> HandleAsync(GetTodoItemByIdRequest request, CancellationToken cancellationToken)
    {
        var todoItem = await todoItems.GetByIdAsync(request.Id, cancellationToken);

        if (todoItem is null)
        {
            return Result.Failure<TodoItemResponse>(
                ResultError.NotFound(ResultErrorCodes.NotFound, $"Todo '{request.Id}' was not found."));
        }

        var response = new TodoItemResponse
        {
            Id = todoItem.Id,
            Title = todoItem.Title,
            Description = todoItem.Description,
            DueDate = todoItem.DueDate,
            IsCompleted = todoItem.IsCompleted,
        };

        return Result.Success(response);
    }
}
