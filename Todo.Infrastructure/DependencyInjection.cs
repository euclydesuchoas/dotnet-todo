using FluentMigrator.Runner;
using FluentMigrator.Runner.Conventions;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Todo.Application.Abstractions.Persistence;
using Todo.Infrastructure.Common.Options;
using Todo.Infrastructure.Persistence;
using Todo.Infrastructure.Persistence.Migrations;
using Todo.Infrastructure.Persistence.Migrations.Conventions;

namespace Todo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, Assembly? assembly = null)
    {
        assembly ??= typeof(DependencyInjection).Assembly;

        var databaseOptions = AddDatabaseOptions(services, configuration);

        // Add infrastructure services to the container.
        services.AddDbContext<TodoDbContext>(builder => builder.UseDatabaseProvider(databaseOptions));

        // O próprio contexto é a unidade de trabalho: quem grava é o SaveChanges dele,
        // e uma segunda implementação só acrescentaria uma camada sem comportamento.
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<TodoDbContext>());

        services.AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddDatabaseProvider(databaseOptions.Provider)
                .WithGlobalConnectionString(databaseOptions.ConnectionString)
                .ScanIn(assembly).For.Migrations())
            .AddLogging(logging => logging.AddFluentMigratorConsole())
            // Registrados depois do AddFluentMigratorCore para sobrepor os padrões do pacote:
            // a última implementação registrada é a que ele resolve.
            .AddSingleton<IConventionSet, MigrationConventionSet>()
            .AddScoped<IVersionTableMetaData, MigrationVersionTable>();

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository", StringComparison.Ordinal)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    /// <remarks>
    /// A seção é lida duas vezes de propósito. O registro precisa do provider agora, porque é
    /// ele que decide o que entra no container, e nesse momento o <c>ValidateOnStart</c> ainda
    /// não rodou — daí a validação explícita, que devolve a mesma mensagem das anotações em vez
    /// de deixar o provider falhar mais adiante com um erro de conexão obscuro.
    /// </remarks>
    private static DatabaseOptions AddDatabaseOptions(IServiceCollection services, IConfiguration configuration)
    {
        // Configuração inválida derruba a aplicação no boot, e não na primeira requisição.
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var databaseOptions = configuration.GetSection(DatabaseOptions.SectionName).Get<DatabaseOptions>()
            ?? throw new InvalidOperationException($"The '{DatabaseOptions.SectionName}' configuration section is required.");

        Validator.ValidateObject(databaseOptions, new ValidationContext(databaseOptions), validateAllProperties: true);

        return databaseOptions;
    }
}
