using Todo.Application.Abstractions.Messaging;

namespace Todo.Application.Todos.GetTodoById;

public sealed record GetTodoByIdRequest(
    Guid Id
) : IRequest<TodoResponse>;
