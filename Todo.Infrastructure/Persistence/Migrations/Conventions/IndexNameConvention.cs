using FluentMigrator.Expressions;
using FluentMigrator.Runner.Conventions;

namespace Todo.Infrastructure.Persistence.Migrations.Conventions;

internal sealed class IndexNameConvention : IIndexConvention
{
    public IIndexExpression Apply(IIndexExpression expression)
    {
        var index = expression.Index;

        if (!string.IsNullOrEmpty(index.Name))
        {
            return expression;
        }

        index.Name = MigrationNaming.Index(index.TableName, index.Columns.Select(column => column.Name));

        return expression;
    }
}
