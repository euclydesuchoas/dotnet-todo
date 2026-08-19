using System.Reflection;
using Todo.Api.Endpoints;
using Todo.Application.Todos.GetTodos;
using Todo.Domain.Todos;
using Todo.Infrastructure.Persistence;
using Todo.Shared.Time;

namespace Todo.Tests.Architecture;

/// <summary>
/// Prende a direção das dependências entre as camadas.
/// </summary>
/// <remarks>
/// São duas verificações complementares. A do <c>.csproj</c> pega a referência no momento em
/// que ela é declarada, mesmo antes de qualquer código usá-la. A da assembly compilada pega o
/// que sobrou no IL, inclusive o que entra por caminho indireto — nenhuma das duas sozinha
/// cobre os dois casos.
/// </remarks>
public sealed class LayerDependencyTests
{
    public static TheoryData<string> Layers => [.. SolutionLayers.Ordered];

    [Theory]
    [MemberData(nameof(Layers))]
    public void Layer_declares_only_the_references_it_is_allowed_to_have(string layer)
    {
        var allowed = SolutionLayers.AllowedReferencesOf(layer);

        var forbidden = SolutionLayers.DeclaredReferencesOf(layer)
            .Where(reference => !allowed.Contains(reference))
            .Order()
            .ToArray();

        Assert.True(
            forbidden.Length == 0,
            $"{layer} declara ProjectReference proibido: {string.Join(", ", forbidden)}. Permitido: {string.Join(", ", allowed.Order())}.");
    }

    [Theory]
    [MemberData(nameof(Layers))]
    public void Compiled_layer_does_not_reach_a_layer_above_it(string layer)
    {
        var allowed = SolutionLayers.AllowedReferencesOf(layer);

        var forbidden = AssemblyOf(layer)
            .GetReferencedAssemblies()
            .Select(reference => reference.Name!)
            .Where(name => SolutionLayers.Ordered.Contains(name) && name != layer && !allowed.Contains(name))
            .Order()
            .ToArray();

        Assert.True(
            forbidden.Length == 0,
            $"A assembly {layer} referencia camada acima dela: {string.Join(", ", forbidden)}.");
    }

    /// <remarks>
    /// O domínio não tem referência de projeto nenhuma — nem para o <c>Todo.Shared</c>, que hoje
    /// ele não usa. Está explícito para que a primeira tentativa de apoiá-lo em outra camada
    /// apareça como falha, e não como decisão silenciosa. No dia em que o domínio precisar do
    /// compartilhado de verdade, este teste vira "referencia no máximo o Todo.Shared", e a
    /// mudança é a decisão consciente que ele existe para forçar.
    /// </remarks>
    [Fact]
    public void Domain_stands_alone()
    {
        Assert.Empty(SolutionLayers.DeclaredReferencesOf("Todo.Domain"));
    }

    /// <remarks>
    /// O compartilhado é a base: se ele apontar para qualquer outro projeto, deixa de poder ser
    /// referenciado por todos sem criar ciclo.
    /// </remarks>
    [Fact]
    public void Shared_is_the_base_and_depends_on_no_project()
    {
        Assert.Empty(SolutionLayers.DeclaredReferencesOf("Todo.Shared"));
    }

    /// <remarks>
    /// Sem isto o resto passaria de graça: um parser que não achasse o arquivo, ou que lesse
    /// um elemento com outro nome, devolveria conjunto vazio e nenhuma regra acima acusaria
    /// nada. Aqui o conjunto esperado é conhecido e não vazio.
    /// </remarks>
    [Fact]
    public void Reading_the_project_file_actually_finds_the_references()
    {
        Assert.Equal(
            ["Todo.Application", "Todo.Domain", "Todo.Infrastructure", "Todo.Shared"],
            SolutionLayers.DeclaredReferencesOf("Todo.Api").Order());
    }

    private static Assembly AssemblyOf(string layer)
    {
        return layer switch
        {
            "Todo.Shared" => typeof(UtcDateTime).Assembly,
            "Todo.Domain" => typeof(TodoItem).Assembly,
            "Todo.Application" => typeof(GetTodosRequest).Assembly,
            "Todo.Infrastructure" => typeof(TodoDbContext).Assembly,
            "Todo.Api" => typeof(ApiEndpointGroup).Assembly,
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Camada sem assembly mapeada."),
        };
    }
}
