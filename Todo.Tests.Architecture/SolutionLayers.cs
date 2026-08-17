using System.Xml.Linq;

namespace Todo.Tests.Architecture;

/// <summary>
/// As camadas da solução e o que cada uma pode referenciar.
/// </summary>
/// <remarks>
/// A ordem da lista é a da dependência permitida: cada camada só pode apontar para as
/// anteriores. Camada nova entra aqui, e os testes passam a cobri-la sem mudança de código.
/// </remarks>
internal static class SolutionLayers
{
    internal static readonly string[] Ordered =
    [
        "Todo.Domain",
        "Todo.Application",
        "Todo.Infrastructure",
        "Todo.Api",
    ];

    /// <summary>
    /// Projetos que <paramref name="layer"/> pode referenciar: os que vêm antes dele.
    /// </summary>
    internal static IReadOnlySet<string> AllowedReferencesOf(string layer)
    {
        return Ordered.TakeWhile(name => name != layer).ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Nomes de <c>ProjectReference</c> declarados no <c>.csproj</c> da camada.
    /// </summary>
    /// <remarks>
    /// Lê o arquivo de projeto, e não os metadados da assembly compilada, porque o compilador
    /// omite referência que o código não chega a usar: uma dependência recém-adicionada e ainda
    /// não exercitada não apareceria na assembly, e é justamente aí que se quer barrar.
    /// </remarks>
    internal static IReadOnlySet<string> DeclaredReferencesOf(string layer)
    {
        var project = XDocument.Load(Path.Combine(Root.FullName, layer, $"{layer}.csproj"));

        return project
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")!.Value)
            .Select(path => Path.GetFileNameWithoutExtension(path.Replace('\\', Path.DirectorySeparatorChar)))
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Raiz da solução, alcançada a partir do diretório de saída dos testes.
    /// </summary>
    private static DirectoryInfo Root { get; } = Find();

    private static DirectoryInfo Find()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Todo.slnx")))
        {
            directory = directory.Parent;
        }

        return directory
            ?? throw new InvalidOperationException("Todo.slnx não foi encontrado a partir do diretório de saída dos testes.");
    }
}
