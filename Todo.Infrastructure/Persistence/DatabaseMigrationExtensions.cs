using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Todo.Infrastructure.Common.Options;

namespace Todo.Infrastructure.Persistence;

public static class DatabaseMigrationExtensions
{
    /// <summary>
    /// Aplica as migrations pendentes quando <see cref="DatabaseOptions.ApplyMigrationsOnStartup"/>
    /// estiver ligado; caso contrário não toca no banco.
    /// </summary>
    /// <remarks>
    /// Recebe <see cref="IServiceProvider"/>, e não o app, para que a camada de infraestrutura
    /// não precise conhecer o pipeline do ASP.NET Core.
    /// </remarks>
    public static IServiceProvider ApplyDatabaseMigrations(this IServiceProvider services)
    {
        var options = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        if (!options.ApplyMigrationsOnStartup)
        {
            return services;
        }

        // O runner é registrado como scoped, então precisa de um escopo próprio no boot.
        using var scope = services.CreateScope();

        scope.ServiceProvider.GetRequiredService<IMigrationRunner>().MigrateUp();

        return services;
    }
}
