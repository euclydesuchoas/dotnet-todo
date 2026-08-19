using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.Options;

namespace Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Renomeia a tabela de controle de versão do FluentMigrator para o padrão do schema.
/// </summary>
/// <remarks>
/// O padrão do pacote é <c>VersionInfo</c>, com maiúsculas — o único objeto do banco que
/// escaparia da regra de identificadores em minúsculas, e que no Postgres só seria alcançável
/// entre aspas.
/// </remarks>
[VersionTableMetaData]
internal sealed class MigrationVersionTable(IConventionSet conventionSet, IOptions<RunnerOptions> runnerOptions)
    : DefaultVersionTableMetaData(conventionSet, runnerOptions)
{
    public override string TableName => "version_info";

    public override string ColumnName => "version";

    public override string AppliedOnColumnName => "applied_on";

    public override string DescriptionColumnName => "description";

    public override string UniqueIndexName => "uc_version_info_version";
}
