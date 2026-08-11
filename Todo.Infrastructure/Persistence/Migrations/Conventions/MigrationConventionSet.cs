using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;

namespace Todo.Infrastructure.Persistence.Migrations.Conventions;

/// <summary>
/// Substitui as convenções de nomenclatura do FluentMigrator pelas do projeto, mantendo o
/// restante do <see cref="DefaultConventionSet"/> intacto.
/// </summary>
/// <remarks>
/// Sem isso, uma constraint sem nome explícito sai sem nome nenhum no SQL, e cada banco
/// inventa o seu: <c>todos_pkey</c> no Postgres, <c>PK__todos__3213E83F...</c> com sufixo
/// aleatório no SQL Server. Um <c>DROP CONSTRAINT</c> em migration futura viraria consulta ao
/// catálogo, com nome diferente em cada ambiente.
///
/// Estas convenções são aplicadas em tempo de execução, então valem para todas as migrations,
/// inclusive as já escritas. Trate-as como congeladas: alterá-las muda o nome que uma migration
/// antiga gera em um banco novo, sem mudar o nome que ela já gerou nos bancos existentes. Para
/// um nome fora do padrão, nomeie a constraint na migration — nome explícito sempre vence.
/// </remarks>
internal sealed class MigrationConventionSet : IConventionSet
{
    public MigrationConventionSet()
        : this(new DefaultConventionSet())
    {
    }

    private MigrationConventionSet(IConventionSet defaults)
    {
        ColumnsConventions = [new PrimaryKeyNameConvention()];

        ConstraintConventions = [new ConstraintNameConvention(), defaults.SchemaConvention];

        ForeignKeyConventions = [new ForeignKeyNameConvention(), defaults.SchemaConvention];

        IndexConventions = [new IndexNameConvention(), defaults.SchemaConvention];

        SequenceConventions = defaults.SequenceConventions;
        AutoNameConventions = defaults.AutoNameConventions;
        SchemaConvention = defaults.SchemaConvention;
        RootPathConvention = defaults.RootPathConvention;
    }

    public IList<IColumnsConvention> ColumnsConventions { get; }

    public IList<IConstraintConvention> ConstraintConventions { get; }

    public IList<IForeignKeyConvention> ForeignKeyConventions { get; }

    public IList<IIndexConvention> IndexConventions { get; }

    public IList<ISequenceConvention> SequenceConventions { get; }

    public IList<IAutoNameConvention> AutoNameConventions { get; }

    public DefaultSchemaConvention SchemaConvention { get; }

    public IRootPathConvention RootPathConvention { get; }
}
