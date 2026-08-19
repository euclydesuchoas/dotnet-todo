using FluentMigrator.Expressions;
using FluentMigrator.Runner.Conventions;

namespace Todo.Infrastructure.Persistence.Migrations.Conventions;

/// <summary>
/// Nomeia a chave primária declarada na própria coluna (<c>.PrimaryKey()</c>).
/// </summary>
internal sealed class PrimaryKeyNameConvention : IColumnsConvention
{
    public IColumnsExpression Apply(IColumnsExpression expression)
    {
        foreach (var column in expression.Columns)
        {
            if (!column.IsPrimaryKey || !string.IsNullOrEmpty(column.PrimaryKeyName))
            {
                continue;
            }

            var tableName = string.IsNullOrEmpty(column.TableName) ? expression.TableName : column.TableName;

            column.PrimaryKeyName = MigrationNaming.PrimaryKey(tableName);
        }

        return expression;
    }
}
