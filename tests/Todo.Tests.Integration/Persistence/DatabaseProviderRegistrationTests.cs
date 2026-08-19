using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todo.Application;
using Todo.Application.Abstractions.Persistence;
using Todo.Domain.TodoItems;
using Todo.Infrastructure;
using Todo.Infrastructure.Persistence;

namespace Todo.Tests.Integration.Persistence;

/// <summary>
/// Verifica, sem servidor de banco, que os três providers suportados chegam a um modelo
/// válido e a um runner de migration configurado.
/// </summary>
/// <remarks>
/// Nada aqui abre conexão: o EF Core valida e compila o modelo para o provider escolhido,
/// e o runner do FluentMigrator é construído. É o que dá para exigir de Postgres e SQL Server
/// em uma máquina qualquer — a execução real do schema depende de servidor.
/// </remarks>
public sealed class DatabaseProviderRegistrationTests
{
    [Theory]
    [InlineData("Postgres", "Host=localhost;Database=todo;Username=todo;Password=todo")]
    [InlineData("SqlServer", "Server=localhost;Database=todo;Trusted_Connection=True;TrustServerCertificate=True")]
    [InlineData("Sqlite", "Data Source=:memory:")]
    public void Every_supported_provider_builds_a_valid_model(string provider, string connectionString)
    {
        using var services = BuildServices(provider, connectionString);
        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        // Acessar o modelo força sua construção e validação para o provider corrente.
        var todoItemEntity = dbContext.Model.FindEntityType(typeof(TodoItem));

        Assert.NotNull(todoItemEntity);
        Assert.Equal("todos", todoItemEntity.GetTableName());

        var columnNames = todoItemEntity.GetProperties().Select(property => property.GetColumnName());

        Assert.Equal(["description", "due_date", "id", "is_completed", "title"], columnNames.Order());

        // Tripwire de drift: estes são os tamanhos que a migration 202608100001 gravou no banco.
        // Se o domínio aumentar um limite, o teste quebra aqui — e a correção é uma migration
        // nova de alter, não um ajuste destes números.
        Assert.Equal(100, todoItemEntity.FindProperty(nameof(TodoItem.Title))!.GetMaxLength());
        Assert.Equal(500, todoItemEntity.FindProperty(nameof(TodoItem.Description))!.GetMaxLength());

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IMigrationRunner>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<ITodoItemRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
    }

    [Fact]
    public void Missing_provider_fails_at_registration()
    {
        var exception = Record.Exception(() => BuildServices(provider: string.Empty, connectionString: "Data Source=:memory:"));

        Assert.NotNull(exception);
    }

    [Fact]
    public void Missing_section_fails_at_registration()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Record.Exception(() => new ServiceCollection().AddInfrastructure(configuration));

        Assert.IsType<InvalidOperationException>(exception);
    }

    private static ServiceProvider BuildServices(string provider, string connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = provider,
                ["Database:ConnectionString"] = connectionString,
            })
            .Build();

        return new ServiceCollection()
            .AddApplication()
            .AddInfrastructure(configuration)
            .BuildServiceProvider();
    }
}
