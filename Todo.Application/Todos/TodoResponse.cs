namespace Todo.Application.Todos;

public sealed class TodoResponse
{
    public Guid Id { get; init; }

    public string Title { get; init; } = null!;

    public string Description { get; init; } = null!;

    public DateTime DueDate { get; init; }

    public bool IsCompleted { get; init; }
}
