using FluentMigrator.Expressions;
using FluentMigrator.Runner.Conventions;

namespace Todo.Infrastructure.Persistence.Migrations.Conventions;

internal sealed class ForeignKeyNameConvention : IForeignKeyConvention
{
    public IForeignKeyExpression Apply(IForeignKeyExpression expression)
    {
        var foreignKey = expression.ForeignKey;

        if (!string.IsNullOrEmpty(foreignKey.Name))
        {
            return expression;
        }

        foreignKey.Name = MigrationNaming.ForeignKey(
            foreignKey.ForeignTable,
            foreignKey.ForeignColumns,
            foreignKey.PrimaryTable,
            foreignKey.PrimaryColumns);

        return expression;
    }
}
