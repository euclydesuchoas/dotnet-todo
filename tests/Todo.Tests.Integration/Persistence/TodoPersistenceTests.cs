using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todo.Application;
using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;
using Todo.Application.Todos;
using Todo.Application.Todos.CreateTodo;
using Todo.Application.Todos.GetTodoById;
using Todo.Application.Todos.GetTodos;
using Todo.Infrastructure;
using Todo.Infrastructure.Persistence;

namespace Todo.Tests.Integration.Persistence;

/// <summary>
/// Exercita a persistência de ponta a ponta contra um SQLite em arquivo temporário:
/// migration aplicada pelo FluentMigrator, gravação e leitura pelo EF Core.
/// </summary>
/// <remarks>
/// SQLite é o único dos três providers que roda sem servidor, e por isso é o que dá para
/// verificar de verdade em qualquer máquina. A compatibilidade dos outros dois é coberta
/// por <see cref="DatabaseProviderRegistrationTests"/>, que valida o mapeamento sem conectar.
/// </remarks>
public sealed class TodoPersistenceTests : IAsyncLifetime
{
    private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"todo-tests-{Guid.NewGuid():N}.db");

    private ServiceProvider services = null!;

    public ValueTask InitializeAsync()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = "Sqlite",
                ["Database:ConnectionString"] = $"Data Source={databasePath}",
                ["Database:ApplyMigrationsOnStartup"] = "true",
            })
            .Build();

        services = new ServiceCollection()
            .AddApplication()
            .AddInfrastructure(configuration)
            .BuildServiceProvider();

        services.ApplyDatabaseMigrations();

        return ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await services.DisposeAsync();

        // Descartar o container não basta: o Microsoft.Data.Sqlite mantém as conexões em pool,
        // e o arquivo segue aberto — e indeletável no Windows — até o pool ser esvaziado.
        SqliteConnection.ClearAllPools();

        File.Delete(databasePath);
    }

    [Fact]
    public async Task Created_todo_is_read_back_with_the_same_values()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var dueDate = DateTime.UtcNow.AddDays(1);

        var request = new CreateTodoRequest("Ler o histórico", "Conferir os commits da semana", dueDate, false);

        var createResult = await HandleAsync<CreateTodoRequest, Guid>(request, cancellationToken);

        Assert.True(createResult.IsSuccess);
        Assert.NotEqual(Guid.Empty, createResult.Data);

        var readResult = await HandleAsync<GetTodoByIdRequest, TodoResponse>(
            new GetTodoByIdRequest(createResult.Data), cancellationToken);

        Assert.True(readResult.IsSuccess);

        var todo = readResult.Data!;

        Assert.Equal(createResult.Data, todo.Id);
        Assert.Equal(request.Title, todo.Title);
        Assert.Equal(request.Description, todo.Description);
        Assert.False(todo.IsCompleted);

        // O instante precisa voltar já marcado como UTC, e não como Unspecified: é isso que
        // mantém a mesma leitura nos três bancos, apesar de só o Postgres guardar o fuso.
        Assert.Equal(DateTimeKind.Utc, todo.DueDate.Kind);
        Assert.Equal(dueDate, todo.DueDate, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task Missing_todo_fails_as_not_found()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var readResult = await HandleAsync<GetTodoByIdRequest, TodoResponse>(
            new GetTodoByIdRequest(Guid.CreateVersion7()), cancellationToken);

        Assert.True(readResult.IsFailure);
        Assert.Equal(ResultErrorTypeEnum.NotFound, readResult.Error.Type);
    }

    /// <remarks>
    /// O filtro compara instantes, então o mesmo momento escrito com offsets diferentes tem
    /// que selecionar exatamente as mesmas tarefas. Aqui o request é montado direto, já
    /// normalizado; o caminho HTTP é coberto pelo filtro de endpoint da API.
    /// </remarks>
    [Fact]
    public async Task Due_date_filter_selects_by_instant_regardless_of_offset()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var early = new DateTime(2027, 3, 10, 9, 0, 0, DateTimeKind.Utc);
        var late = new DateTime(2027, 3, 10, 15, 0, 0, DateTimeKind.Utc);

        foreach (var dueDate in new[] { early, late })
        {
            var created = await HandleAsync<CreateTodoRequest, Guid>(
                new CreateTodoRequest($"Tarefa {dueDate:HH:mm}", "Descrição", dueDate, false), cancellationToken);

            Assert.True(created.IsSuccess);
        }

        // As três grafias do mesmo instante: UTC, offset negativo e offset positivo.
        DateTime[] boundaries =
        [
            new DateTime(2027, 3, 10, 12, 0, 0, DateTimeKind.Utc),
            new DateTimeOffset(2027, 3, 10, 9, 0, 0, TimeSpan.FromHours(-3)).UtcDateTime,
            new DateTimeOffset(2027, 3, 10, 21, 0, 0, TimeSpan.FromHours(9)).UtcDateTime,
        ];

        foreach (var boundary in boundaries)
        {
            var listed = await HandleAsync<GetTodosRequest, IReadOnlyList<TodoResponse>>(
                new GetTodosRequest(DueFrom: boundary), cancellationToken);

            Assert.True(listed.IsSuccess);

            var only = Assert.Single(listed.Data!);

            Assert.Equal(late, only.DueDate);
        }
    }

    [Fact]
    public async Task Listing_filters_combine_and_are_optional()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await HandleAsync<CreateTodoRequest, Guid>(
            new CreateTodoRequest("Comprar pão", "Padaria", new DateTime(2027, 3, 10, 9, 0, 0, DateTimeKind.Utc), true),
            cancellationToken);

        await HandleAsync<CreateTodoRequest, Guid>(
            new CreateTodoRequest("Comprar leite", "Mercado", new DateTime(2027, 3, 11, 9, 0, 0, DateTimeKind.Utc), false),
            cancellationToken);

        var all = await HandleAsync<GetTodosRequest, IReadOnlyList<TodoResponse>>(
            new GetTodosRequest(), cancellationToken);

        Assert.Equal(2, all.Data!.Count);

        var byTitle = await HandleAsync<GetTodosRequest, IReadOnlyList<TodoResponse>>(
            new GetTodosRequest(Title: "leite"), cancellationToken);

        Assert.Equal("Comprar leite", Assert.Single(byTitle.Data!).Title);

        var completed = await HandleAsync<GetTodosRequest, IReadOnlyList<TodoResponse>>(
            new GetTodosRequest(IsCompleted: true), cancellationToken);

        Assert.Equal("Comprar pão", Assert.Single(completed.Data!).Title);

        // Intervalo fechado dos dois lados: o limite superior inclui a tarefa que cai nele.
        var inRange = await HandleAsync<GetTodosRequest, IReadOnlyList<TodoResponse>>(
            new GetTodosRequest(
                DueFrom: new DateTime(2027, 3, 11, 9, 0, 0, DateTimeKind.Utc),
                DueTo: new DateTime(2027, 3, 11, 9, 0, 0, DateTimeKind.Utc)),
            cancellationToken);

        Assert.Equal("Comprar leite", Assert.Single(inRange.Data!).Title);
    }

    [Fact]
    public async Task Listing_rejects_an_inverted_due_date_range()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await HandleAsync<GetTodosRequest, IReadOnlyList<TodoResponse>>(
            new GetTodosRequest(
                DueFrom: new DateTime(2027, 3, 11, 9, 0, 0, DateTimeKind.Utc),
                DueTo: new DateTime(2027, 3, 10, 9, 0, 0, DateTimeKind.Utc)),
            cancellationToken);

        Assert.True(result.IsFailure);
        Assert.Equal(ResultErrorTypeEnum.Validation, result.Error.Type);
    }

    [Fact]
    public async Task Schema_follows_the_project_naming_conventions()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        using var scope = services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

        var todosSchema = await dbContext.Database
            .SqlQuery<string>($"SELECT sql AS Value FROM sqlite_master WHERE name = 'todos'")
            .SingleAsync(cancellationToken);

        // A migration não nomeia a chave primária: quem nomeia é o MigrationConventionSet.
        Assert.Contains("""CONSTRAINT "pk_todos" PRIMARY KEY""", todosSchema);

        var versionTableSchema = await dbContext.Database
            .SqlQuery<string>($"SELECT sql AS Value FROM sqlite_master WHERE type = 'table' AND name = 'version_info'")
            .SingleOrDefaultAsync(cancellationToken);

        // A tabela de controle do FluentMigrator seria "VersionInfo" sem o MigrationVersionTable.
        Assert.NotNull(versionTableSchema);
        Assert.Contains("version", versionTableSchema);
        Assert.Contains("applied_on", versionTableSchema);
        Assert.Contains("description", versionTableSchema);

        var versionIndexName = await dbContext.Database
            .SqlQuery<string>($"SELECT name AS Value FROM sqlite_master WHERE type = 'index' AND tbl_name = 'version_info'")
            .SingleAsync(cancellationToken);

        Assert.Equal("uc_version_info_version", versionIndexName);
    }

    // Cada caso de uso é resolvido em seu próprio escopo, como aconteceria em requisições distintas.
    private async Task<Result<TResponse>> HandleAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        using var scope = services.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<IServiceHandler<TRequest, TResponse>>();

        return await handler.HandleAsync(request, cancellationToken);
    }
}
