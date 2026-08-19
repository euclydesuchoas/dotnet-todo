using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todo.Infrastructure;
using Todo.Infrastructure.Persistence;

namespace Todo.Tests.Integration.Persistence;

/// <summary>
/// Cobre a criação do banco no boot, contra um SQLite em arquivo temporário.
/// </summary>
/// <remarks>
/// O SQLite serve de banco de prova aqui porque a existência dele é visível pelo sistema de
/// arquivos: o arquivo estar ou não estar lá responde à pergunta sem precisar de servidor.
/// </remarks>
public sealed class DatabaseCreationTests : IDisposable
{
    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"todo-tests-{Guid.NewGuid():N}.db");

    [Fact]
    public void Database_is_created_when_it_does_not_exist_yet()
    {
        using var services = Build(createDatabaseOnStartup: true);

        Assert.False(File.Exists(databasePath));

        services.EnsureDatabaseExists();

        Assert.True(File.Exists(databasePath));
    }

    /// <remarks>
    /// O segundo boot passa pelo mesmo caminho, e criar um banco que já existe é erro em
    /// qualquer um dos três providers.
    /// </remarks>
    [Fact]
    public void Creating_an_existing_database_is_harmless()
    {
        using var services = Build(createDatabaseOnStartup: true);

        services.EnsureDatabaseExists();
        services.EnsureDatabaseExists();

        Assert.True(File.Exists(databasePath));
    }

    /// <remarks>
    /// Desligado é o padrão, e é o que vale em produção: quem cria o banco lá é o passo de
    /// provisionamento, com credencial própria.
    /// </remarks>
    [Fact]
    public void Database_is_left_alone_when_the_option_is_off()
    {
        using var services = Build(createDatabaseOnStartup: false);

        services.EnsureDatabaseExists();

        Assert.False(File.Exists(databasePath));
    }

    public void Dispose()
    {
        // O Microsoft.Data.Sqlite mantém as conexões em pool, e o arquivo segue aberto — e
        // indeletável no Windows — até o pool ser esvaziado.
        SqliteConnection.ClearAllPools();

        File.Delete(databasePath);
    }

    private ServiceProvider Build(bool createDatabaseOnStartup)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = "Sqlite",
                ["Database:ConnectionString"] = $"Data Source={databasePath}",
                ["Database:CreateDatabaseOnStartup"] = createDatabaseOnStartup ? "true" : "false",
            })
            .Build();

        return new ServiceCollection()
            .AddInfrastructure(configuration)
            .BuildServiceProvider();
    }
}
