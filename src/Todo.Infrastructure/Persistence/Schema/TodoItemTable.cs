namespace Todo.Infrastructure.Persistence.Schema;

/// <summary>
/// Nomes físicos da tabela de tarefas, na forma como o mapeamento do EF Core os enxerga.
/// </summary>
/// <remarks>
/// As migrations não leem daqui: cada uma repete os nomes como literais, porque descreve o
/// schema de um momento no tempo e não pode mudar de resultado quando estas constantes
/// mudarem. Renomear uma coluna é alterar este arquivo e escrever a migration de rename.
///
/// Os nomes estão em minúsculas de propósito. O Postgres rebaixa identificadores não citados
/// para minúsculas, então um nome com maiúsculas só é alcançável entre aspas — o que funciona
/// pelo EF e pelo FluentMigrator, mas atrapalha quem for consultar a tabela à mão. Minúsculas
/// significam o mesmo nome nos três bancos, citado ou não.
/// </remarks>
internal static class TodoItemTable
{
    internal const string Name = "todos";

    internal const string PrimaryKeyName = "pk_todos";

    internal static class Columns
    {
        internal const string Id = "id";

        internal const string Title = "title";

        internal const string Description = "description";

        internal const string DueDate = "due_date";

        internal const string IsCompleted = "is_completed";
    }
}
