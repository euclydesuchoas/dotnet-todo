using Todo.Application.Abstractions.Messaging;

namespace Todo.Application.TodoItems.CreateTodoItem;

public sealed record CreateTodoItemRequest(
    string Title,
    string Description,
    DateTime DueDate,
    bool IsCompleted
) : IRequest<Guid>;
