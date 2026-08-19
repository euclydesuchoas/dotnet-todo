using System.Reflection;
using System.Xml.Linq;
using Todo.Api.Endpoints;
using Todo.Application.Todos.GetTodos;
using Todo.Domain.Todos;
using Todo.Infrastructure.Persistence;
using Todo.Shared.Time;

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
    private const string SolutionFileName = "Todo.slnx";

    internal static readonly string[] Ordered =
    [
        "Todo.Shared",
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
        var project = XDocument.Load(ProjectFileOf(layer));

        return project
            .Descendants("ProjectReference")
            .Select(reference => reference.Attribute("Include")!.Value)
            .Select(path => Path.GetFileNameWithoutExtension(path.Replace('\\', Path.DirectorySeparatorChar)))
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Assembly compilada da camada.
    /// </summary>
    /// <remarks>
    /// O mapa é explícito, com um tipo âncora por camada, e não uma busca por nome no domínio
    /// da aplicação: a assembly só aparece lá depois de carregada, e o que dispara a carga é o
    /// primeiro teste que toca nela — resultado dependente da ordem de execução.
    /// </remarks>
    internal static Assembly AssemblyOf(string layer)
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

    /// <summary>
    /// Raiz da solução, alcançada a partir do diretório de saída dos testes.
    /// </summary>
    private static DirectoryInfo Root { get; } = Find();

    /// <summary>
    /// Caminho do <c>.csproj</c> de cada projeto, relativo à raiz, como a solução o declara.
    /// </summary>
    /// <remarks>
    /// A solução é quem sabe onde cada projeto mora, então reorganizar as pastas — juntar os
    /// projetos em <c>src/</c> e <c>tests/</c>, por exemplo — não obriga a mexer aqui.
    /// </remarks>
    private static IReadOnlyDictionary<string, string> Declared { get; } = ReadDeclared();

    private static DirectoryInfo Find()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
        {
            directory = directory.Parent;
        }

        return directory
            ?? throw new InvalidOperationException($"{SolutionFileName} não foi encontrado a partir do diretório de saída dos testes.");
    }

    private static IReadOnlyDictionary<string, string> ReadDeclared()
    {
        var solution = XDocument.Load(Path.Combine(Root.FullName, SolutionFileName));

        return solution
            .Descendants("Project")
            .Select(project => project.Attribute("Path")!.Value.Replace('\\', Path.DirectorySeparatorChar))
            .ToDictionary(path => Path.GetFileNameWithoutExtension(path)!, StringComparer.Ordinal);
    }

    private static string ProjectFileOf(string layer)
    {
        if (!Declared.TryGetValue(layer, out var path))
        {
            throw new InvalidOperationException($"{layer} não está declarado em {SolutionFileName}.");
        }

        return Path.Combine(Root.FullName, path);
    }
}
