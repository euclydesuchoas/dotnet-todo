namespace Todo.Application.TodoItems;

public sealed class TodoItemResponse
{
    public Guid Id { get; init; }

    public string Title { get; init; } = null!;

    public string Description { get; init; } = null!;

    public DateTime DueDate { get; init; }

    public bool IsCompleted { get; init; }
}
