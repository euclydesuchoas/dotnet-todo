using Todo.Application.Abstractions.Messaging;
using Todo.Application.Abstractions.Persistence;
using Todo.Application.Common.Results;
using Todo.Domain.TodoItems;

namespace Todo.Application.TodoItems.CreateTodoItem;

internal sealed class CreateTodoItemHandler(ITodoItemRepository todoItems, IUnitOfWork unitOfWork)
    : IServiceHandler<CreateTodoItemRequest, Guid>
{
    public async Task<Result<Guid>> HandleAsync(CreateTodoItemRequest request, CancellationToken cancellationToken)
    {
        var todoItem = TodoItem.Create(request.Title, request.Description, request.DueDate, request.IsCompleted);

        await todoItems.AddAsync(todoItem, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(todoItem.Id);
    }
}
