using Todo.Application.Abstractions.Messaging;
using Todo.Application.Abstractions.Persistence;
using Todo.Application.Common.Results;

namespace Todo.Application.TodoItems.GetTodoItems;

internal sealed class GetTodoItemsHandler(ITodoItemRepository todoItems)
    : IServiceHandler<GetTodoItemsRequest, IReadOnlyList<TodoItemResponse>>
{
    public async Task<Result<IReadOnlyList<TodoItemResponse>>> HandleAsync(GetTodoItemsRequest request, CancellationToken cancellationToken)
    {
        var found = await todoItems.ListAsync(
            request.Title,
            request.IsCompleted,
            request.DueFrom,
            request.DueTo,
            cancellationToken);

        var response = found
            .Select(todoItem => new TodoItemResponse
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                Description = todoItem.Description,
                DueDate = todoItem.DueDate,
                IsCompleted = todoItem.IsCompleted,
            })
            .ToArray();

        return Result.Success<IReadOnlyList<TodoItemResponse>>(response);
    }
}
