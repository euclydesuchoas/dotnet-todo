using Todo.Infrastructure.Common;
using Todo.Infrastructure.Common.Options;

namespace Todo.Tests.Unit.Common;

public sealed class DatabaseProviderMatchExtensionsTests
{
    /// <summary>
    /// Fecha a única folga do desenho: acrescentar um membro a
    /// <see cref="DatabaseProviderEnum"/> sem acrescentar o parâmetro correspondente em
    /// <see cref="DatabaseProviderMatchExtensions"/> compila, porque o novo membro cai no
    /// descarte.
    /// </summary>
    /// <remarks>
    /// Depois que o parâmetro existe, é a assinatura que cobra as chamadas — este teste só
    /// cuida do passo anterior, dentro do próprio switch.
    /// </remarks>
    [Fact]
    public void Every_declared_provider_has_a_branch()
    {
        foreach (var provider in Enum.GetValues<DatabaseProviderEnum>())
        {
            var chosen = provider.Match(
                postgres: () => DatabaseProviderEnum.Postgres,
                sqlServer: () => DatabaseProviderEnum.SqlServer,
                sqlite: () => DatabaseProviderEnum.Sqlite);

            Assert.Equal(provider, chosen);
        }
    }

    [Fact]
    public void Undeclared_provider_is_rejected()
    {
        var exception = Record.Exception(() => ((DatabaseProviderEnum)0).Match(
            postgres: () => 1,
            sqlServer: () => 2,
            sqlite: () => 3));

        Assert.IsType<InvalidOperationException>(exception);
    }

    [Fact]
    public void Only_the_chosen_branch_runs()
    {
        var executed = new List<string>();

        DatabaseProviderEnum.Sqlite.Match(
            postgres: () => executed.Add("postgres"),
            sqlServer: () => executed.Add("sqlServer"),
            sqlite: () => executed.Add("sqlite"));

        Assert.Equal(["sqlite"], executed);
    }
}
