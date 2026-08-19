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

    /// <remarks>
    /// O <c>Todo.Shared</c> entra na lista porque é o candidato natural a virar depósito: como
    /// todos o alcançam, é para lá que um utilitário transversal vai por conveniência. Ele
    /// carrega política do projeto, não tecnologia de borda.
    /// </remarks>
    public static TheoryData<string> InnerLayers => ["Todo.Shared", "Todo.Domain", "Todo.Application"];

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

    public static TheoryData<string> LayersWithoutPackages => ["Todo.Shared", "Todo.Domain"];

    /// <remarks>
    /// Os dois projetos da base não têm pacote nenhum no <c>.csproj</c>, e o teste registra
    /// isso: tudo que referenciam vem da biblioteca base. Nem regra de negócio nem política de
    /// fronteira dependem de escolha de biblioteca, então a primeira que entrar deve ser uma
    /// decisão consciente — e não o efeito colateral de alguém precisar de um utilitário.
    /// </remarks>
    [Theory]
    [MemberData(nameof(LayersWithoutPackages))]
    public void Base_layer_only_depends_on_the_base_library(string layer)
    {
        var external = ReferencesOf(layer)
            .Where(name => !name.StartsWith("System.", StringComparison.Ordinal))
            .Where(name => name is not ("System" or "mscorlib" or "netstandard"))
            .Order()
            .ToArray();

        Assert.True(
            external.Length == 0,
            $"{layer} passou a depender de: {string.Join(", ", external)}.");
    }

    private static IEnumerable<string> ReferencesOf(string layer)
    {
        return SolutionLayers.AssemblyOf(layer).GetReferencedAssemblies().Select(reference => reference.Name!);
    }
}
