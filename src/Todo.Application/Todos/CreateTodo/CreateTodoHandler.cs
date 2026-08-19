using Todo.Application.Abstractions.Messaging;
using Todo.Application.Abstractions.Persistence;
using Todo.Application.Common.Results;
using Todo.Domain.Todos;

namespace Todo.Application.Todos.CreateTodo;

internal sealed class CreateTodoHandler(ITodoRepository todos, IUnitOfWork unitOfWork)
    : IServiceHandler<CreateTodoRequest, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateTodoRequest request, CancellationToken cancellationToken)
    {
        var todo = TodoItem.Create(request.Title, request.Description, request.DueDate, request.IsCompleted);

        await todos.AddAsync(todo, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(todo.Id);
    }
}
