using Todo.Application.Abstractions.Messaging;

namespace Todo.Application.TodoItems.GetTodoItemById;

public sealed record GetTodoItemByIdRequest(
    Guid Id
) : IRequest<TodoItemResponse>;
