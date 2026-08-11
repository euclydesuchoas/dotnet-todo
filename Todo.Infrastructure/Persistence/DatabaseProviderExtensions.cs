using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Todo.Infrastructure.Common;
using Todo.Infrastructure.Common.Options;

namespace Todo.Infrastructure.Persistence;

/// <summary>
/// Único ponto em que <see cref="DatabaseProviderEnum"/> se abre nos providers concretos.
/// </summary>
/// <remarks>
/// São dois caminhos porque são duas ferramentas independentes: o EF Core lê e grava dados,
/// o FluentMigrator aplica o schema. Suportar um novo banco é acrescentar um membro ao enum
/// e um ramo em cada um dos dois métodos — e nada mais no resto da aplicação.
///
/// Quem abre o enum é <see cref="DatabaseProviderMatchExtensions"/>, e não um <c>switch</c>
/// aqui: o parâmetro obrigatório por provider faz o compilador cobrar o ramo novo.
/// </remarks>
internal static class DatabaseProviderExtensions
{
    internal static DbContextOptionsBuilder UseDatabaseProvider(this DbContextOptionsBuilder builder, DatabaseOptions options)
    {
        return options.Provider.Match(
            postgres: () => builder.UseNpgsql(options.ConnectionString),
            sqlServer: () => builder.UseSqlServer(options.ConnectionString),
            sqlite: () => builder.UseSqlite(options.ConnectionString));
    }

    internal static IMigrationRunnerBuilder AddDatabaseProvider(this IMigrationRunnerBuilder builder, DatabaseProviderEnum provider)
    {
        return provider.Match(
            postgres: () => builder.AddPostgres(),
            sqlServer: () => builder.AddSqlServer(),
            sqlite: () => builder.AddSQLite());
    }
}
