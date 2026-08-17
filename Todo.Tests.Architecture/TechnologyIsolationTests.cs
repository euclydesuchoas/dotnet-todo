using System.Reflection;
using Todo.Application.Todos.GetTodos;
using Todo.Domain.Todos;

namespace Todo.Tests.Architecture;

/// <summary>
/// Mantém as tecnologias de borda fora do domínio e da aplicação.
/// </summary>
/// <remarks>
/// A regra de camadas já impede apontar para <c>Todo.Infrastructure</c> e <c>Todo.Api</c>, mas
/// não impede referenciar direto o pacote que elas usam — um <c>DbContext</c> na aplicação ou
/// um atributo de ASP.NET Core no domínio passariam por ela. É o mesmo acoplamento por outro
/// caminho.
/// </remarks>
public sealed class TechnologyIsolationTests
{
    private static readonly string[] EdgeTechnologies =
    [
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Microsoft.Data.Sqlite",
        "Npgsql",
        "FluentMigrator",
    ];

    public static TheoryData<string> InnerLayers => ["Todo.Domain", "Todo.Application"];

    [Theory]
    [MemberData(nameof(InnerLayers))]
    public void Inner_layer_does_not_reference_an_edge_technology(string layer)
    {
        var forbidden = ReferencesOf(layer)
            .Where(name => EdgeTechnologies.Any(technology =>
                name.Equals(technology, StringComparison.Ordinal) ||
                name.StartsWith($"{technology}.", StringComparison.Ordinal)))
            .Order()
            .ToArray();

        Assert.True(
            forbidden.Length == 0,
            $"{layer} referencia tecnologia de borda: {string.Join(", ", forbidden)}.");
    }

    /// <remarks>
    /// O domínio não tem pacote nenhum no <c>.csproj</c>, e o teste registra isso: tudo que ele
    /// referencia vem da biblioteca base. As regras de negócio não dependem de escolha de
    /// biblioteca, então a primeira que entrar deve ser uma decisão consciente.
    /// </remarks>
    [Fact]
    public void Domain_only_depends_on_the_base_library()
    {
        var external = ReferencesOf("Todo.Domain")
            .Where(name => !name.StartsWith("System.", StringComparison.Ordinal))
            .Where(name => name is not ("System" or "mscorlib" or "netstandard"))
            .Order()
            .ToArray();

        Assert.True(
            external.Length == 0,
            $"Todo.Domain passou a depender de: {string.Join(", ", external)}.");
    }

    private static IEnumerable<string> ReferencesOf(string layer)
    {
        var assembly = layer switch
        {
            "Todo.Domain" => typeof(TodoItem).Assembly,
            "Todo.Application" => typeof(GetTodosRequest).Assembly,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Camada sem assembly mapeada."),
        };

        return assembly.GetReferencedAssemblies().Select(reference => reference.Name!);
    }
}
