using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;

namespace Todo.Application.Todos.CreateTodo;

internal sealed class CreateTodoHandler : IServiceHandler<CreateTodoRequest, Guid>
{
    public Task<Result<Guid>> HandleAsync(CreateTodoRequest _, CancellationToken cancellationToken)
    {
        // TODO: Implement the logic to create a new Todo item based on the request data.
        var result = Task.FromResult(Result.Success(Guid.CreateVersion7()));
        return result;
    }
}
