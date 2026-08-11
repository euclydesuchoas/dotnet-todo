using Todo.Domain.Common;

namespace Todo.Domain.Todos;

/// <summary>
/// Tarefa persistida. É a única fonte dos limites de tamanho dos campos, consumidos
/// tanto pela validação da camada de aplicação quanto pelo mapeamento e pela migration.
/// </summary>
public sealed class TodoItem
{
    public const int TitleMaxLength = 100;

    public const int DescriptionMaxLength = 500;

    public Guid Id { get; private set; }

    public string Title { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public DateTime DueDate { get; private set; }

    public bool IsCompleted { get; private set; }

    // Exigido pelo materializador do EF Core, que não passa pela fábrica.
    private TodoItem()
    {
    }

    private TodoItem(Guid id, string title, string description, DateTime dueDate, bool isCompleted)
    {
        Id = id;
        Title = title;
        Description = description;
        DueDate = dueDate;
        IsCompleted = isCompleted;
    }

    /// <remarks>
    /// O identificador é UUID v7: por ser ordenado no tempo, mantém as inserções no fim
    /// do índice da chave primária, em vez de espalhá-las como um UUID v4 faria. Isso
    /// importa nos três bancos suportados, já que em todos a chave primária é a
    /// ordenação física ou o índice mais usado.
    /// </remarks>
    public static TodoItem Create(string title, string description, DateTime dueDate, bool isCompleted)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(description);

        // Normalizar aqui torna "DueDate é UTC" invariante do tipo, e não promessa de quem
        // chama. As bordas de HTTP e de persistência já normalizam, mas nenhuma delas cobre
        // teste, seed ou rotina que construa a tarefa direto.
        return new TodoItem(Guid.CreateVersion7(), title, description, UtcDateTime.Normalize(dueDate), isCompleted);
    }
}
