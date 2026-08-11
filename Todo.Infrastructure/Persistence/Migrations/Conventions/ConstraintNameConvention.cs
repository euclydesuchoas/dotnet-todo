using FluentMigrator.Expressions;
using FluentMigrator.Runner.Conventions;

namespace Todo.Infrastructure.Persistence.Migrations.Conventions;

/// <summary>
/// Nomeia constraints declaradas em bloco (<c>Create.PrimaryKey</c>, <c>Create.UniqueConstraint</c>),
/// que não passam pela convenção de coluna.
/// </summary>
internal sealed class ConstraintNameConvention : IConstraintConvention
{
    public IConstraintExpression Apply(IConstraintExpression expression)
    {
        var constraint = expression.Constraint;

        if (!string.IsNullOrEmpty(constraint.ConstraintName))
        {
            return expression;
        }

        constraint.ConstraintName = constraint.IsPrimaryKeyConstraint
            ? MigrationNaming.PrimaryKey(constraint.TableName)
            : MigrationNaming.Unique(constraint.TableName, constraint.Columns);

        return expression;
    }
}
