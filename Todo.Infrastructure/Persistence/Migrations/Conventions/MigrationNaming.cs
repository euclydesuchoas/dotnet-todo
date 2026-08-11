namespace Todo.Infrastructure.Persistence.Migrations.Conventions;

/// <summary>
/// Formato dos nomes de constraint e índice gerados quando a migration não dá um nome explícito.
/// </summary>
/// <remarks>
/// O Postgres trunca identificadores em 63 caracteres e o SQL Server em 128. Nomes derivados de
/// tabela mais colunas passam disso em tabelas de junção com nomes longos, e duas constraints
/// truncadas para o mesmo prefixo colidem. Quando isso acontecer, nomeie a constraint
/// explicitamente na migration — o nome dado à mão sempre vence a convenção.
/// </remarks>
internal static class MigrationNaming
{
    internal static string PrimaryKey(string tableName)
    {
        return $"pk_{tableName}";
    }

    internal static string Unique(string tableName, IEnumerable<string> columnNames)
    {
        return Compose("uc", tableName, columnNames);
    }

    internal static string Index(string tableName, IEnumerable<string> columnNames)
    {
        return Compose("ix", tableName, columnNames);
    }

    internal static string ForeignKey(
        string foreignTableName,
        IEnumerable<string> foreignColumnNames,
        string primaryTableName,
        IEnumerable<string> primaryColumnNames)
    {
        var foreignSide = Compose("fk", foreignTableName, foreignColumnNames);
        var primarySide = Compose(primaryTableName, primaryColumnNames);

        return $"{foreignSide}_{primarySide}";
    }

    private static string Compose(string prefix, string tableName, IEnumerable<string> columnNames)
    {
        return $"{prefix}_{Compose(tableName, columnNames)}";
    }

    private static string Compose(string tableName, IEnumerable<string> columnNames)
    {
        var columns = string.Join('_', columnNames);

        return columns.Length is 0 ? tableName : $"{tableName}_{columns}";
    }
}
