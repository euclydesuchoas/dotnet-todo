using Todo.Application.Abstractions.Messaging;

namespace Todo.Application.Todos.CreateTodo;

public sealed record CreateTodoRequest(
    string Title,
    string Description,
    DateTime DueDate,
    bool IsCompleted
) : IRequest<Guid>;
