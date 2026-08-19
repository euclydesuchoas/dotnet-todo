using FluentMigrator.Builders;
using FluentMigrator.Infrastructure;
using Todo.Infrastructure.Common;
using Todo.Infrastructure.Common.Options;

namespace Todo.Infrastructure.Persistence.Migrations;

/// <summary>
/// Tipos de coluna que não têm equivalente único nos bancos suportados.
/// </summary>
/// <remarks>
/// Extensão sobre <see cref="IColumnTypeSyntax{TNext}"/>, que é a interface comum a
/// <c>Create.Table</c>, <c>Alter.Table</c> e <c>Alter.Column</c> — a mesma chamada serve nos
/// três, e a cadeia fluente continua de onde parou.
///
/// Cuidado ao mexer: migration é registro histórico, e este código é compartilhado por todas
/// elas. Mudar o que um método daqui emite reescreve retroativamente o schema que as
/// migrations antigas descrevem, e bancos já migrados não acompanham. Se um tipo precisar
/// mudar, o caminho é um método novo — <c>AsUtcDateTimeV2</c> ou equivalente — usado só pelas
/// migrations dali para a frente, com uma migration de alter para os bancos existentes.
/// </remarks>
internal static class MigrationColumnExtensions
{
    /// <summary>
    /// Coluna de instante em UTC.
    /// </summary>
    /// <remarks>
    /// No Postgres é <c>timestamptz</c>: tanto <c>AsDateTime</c> quanto <c>AsDateTime2</c>
    /// geram <c>timestamp</c>, sem fuso, enquanto o Npgsql envia um <c>timestamptz</c> para um
    /// <see cref="DateTime"/> em UTC — o banco aceitaria a conversão implícita, mas deslocando
    /// o instante conforme o <c>TimeZone</c> da sessão. Nos outros dois é <c>DATETIME2</c>,
    /// porque o gerador do SQLite lança em <c>DbType.DateTimeOffset</c>.
    ///
    /// Nenhum dos três guarda fuso: o Postgres normaliza para UTC na entrada, e os outros
    /// dois descartam o <see cref="DateTimeKind"/>. Quem garante que o instante entra e sai
    /// igual é o <see cref="Converters.UtcDateTimeConverter"/>, na borda do mapeamento.
    /// </remarks>
    internal static TNext AsUtcDateTime<TNext>(this IColumnTypeSyntax<TNext> column, DatabaseProviderEnum provider)
        where TNext : IFluentSyntax
    {
        return provider.Match(
            postgres: () => column.AsDateTimeOffset(),
            sqlServer: () => column.AsDateTime2(),
            sqlite: () => column.AsDateTime2());
    }
}
