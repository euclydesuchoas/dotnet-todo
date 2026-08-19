using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Todo.Infrastructure.Common.Options;

namespace Todo.Infrastructure.Persistence;

public static class DatabaseCreationExtensions
{
    /// <summary>
    /// Cria o banco, vazio, quando ele ainda não existe e
    /// <see cref="DatabaseOptions.CreateDatabaseOnStartup"/> estiver ligado.
    /// </summary>
    /// <remarks>
    /// Cria o banco e nada mais: nenhuma tabela, nenhum índice. O schema é assunto das
    /// migrations, e é por isso que aqui se usa <see cref="IRelationalDatabaseCreator.Create"/>
    /// em vez de <c>EnsureCreated</c> — este materializaria o modelo do EF Core por fora do
    /// FluentMigrator, sem registrar nada em <c>version_info</c>, e o boot seguinte tentaria
    /// criar as mesmas tabelas de novo.
    ///
    /// Roda antes de <see cref="DatabaseMigrationExtensions.ApplyDatabaseMigrations"/>, que
    /// precisa de um banco alcançável para gravar a versão aplicada.
    /// </remarks>
    public static IServiceProvider EnsureDatabaseExists(this IServiceProvider services)
    {
        var options = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        if (!options.CreateDatabaseOnStartup)
        {
            return services;
        }

        // O contexto é registrado como scoped, então precisa de um escopo próprio no boot.
        using var scope = services.CreateScope();

        var creator = scope.ServiceProvider
            .GetRequiredService<TodoDbContext>()
            .Database.GetService<IRelationalDatabaseCreator>();

        if (creator.Exists())
        {
            return services;
        }

        try
        {
            creator.Create();
        }
        catch (DbException) when (creator.Exists())
        {
            // Outra instância criou o banco entre a verificação e a criação. O que importa é
            // que ele exista agora, e existe.
        }

        return services;
    }
}
