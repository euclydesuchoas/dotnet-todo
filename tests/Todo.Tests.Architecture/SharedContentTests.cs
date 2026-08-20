using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Todo.Shared.Time;

namespace Todo.Tests.Architecture;

/// <summary>
/// Mantém no <c>Todo.Shared</c> só o que alguma camada de fato usa.
/// </summary>
/// <remarks>
/// Este teste já exigiu dois consumidores, na ideia de que o compartilhado guarda o que duas
/// camadas que não se enxergam precisam, e que um consumidor só significaria que o tipo pertence
/// a ele. A regra foi afrouxada de propósito: o <c>TextValue</c> é política de fronteira sem
/// tecnologia, da mesma natureza do <c>UtcDateTime</c>, e a decisão foi que tipos assim moram no
/// compartilhado pelo que são, e não pelo número de camadas que hoje os alcançam.
///
/// O que se perde precisa ficar dito, porque a mudança não é gratuita: com o limite em um
/// consumidor, este teste deixa de reprovar o utilitário conveniente que alguém põe aqui só
/// porque todas as camadas enxergam o projeto — que era o risco que o critério existia para
/// conter. O que continua valendo é mecânico: sem pacote e sem tecnologia de borda
/// (<see cref="TechnologyIsolationTests"/>), sem referência a outro projeto
/// (<see cref="LayerDependencyTests"/>), e usado por alguém. A partir daqui, "não pertence a
/// camada nenhuma" é julgamento humano na revisão, e não mais uma barreira automática.
///
/// O limite em um ainda pega uma coisa real: tipo público que camada nenhuma usa. No
/// compartilhado isso não é base compartilhada, é código morto.
///
/// A contagem sai da tabela de <c>TypeRef</c> de cada assembly, e não de reflexão sobre os tipos
/// carregados no processo: o <c>TypeRef</c> registra a dependência mesmo quando o uso está no
/// corpo de um método que nenhum teste executa.
/// </remarks>
public sealed class SharedContentTests
{
    private const string Shared = "Todo.Shared";

    private const int RequiredConsumers = 1;

    public static TheoryData<string> SharedTypes => [.. PublicTypesOfShared()];

    [Theory]
    [MemberData(nameof(SharedTypes))]
    public void Shared_type_is_used_by_at_least_one_layer(string type)
    {
        var consumers = ConsumersOf(type);

        Assert.True(
            consumers.Length >= RequiredConsumers,
            $"{type} está no {Shared} e não é usado por camada nenhuma. "
            + "O compartilhado é a base de quem depende dele; um tipo público que ninguém "
            + "alcança não é base compartilhada, é código morto — apague-o ou mova-o para "
            + "onde ele for de fato usado.");
    }

    /// <remarks>
    /// Sem isto a regra acima passaria de graça no dia em que a leitura de metadados deixasse de
    /// achar as referências: conjunto vazio para toda camada, nenhum consumidor contado, e ainda
    /// assim nenhuma falha — porque o teste só sabe reprovar o que ele consegue enxergar.
    /// </remarks>
    [Fact]
    public void Reading_the_metadata_actually_finds_the_shared_types()
    {
        Assert.Contains(typeof(UtcDateTime).FullName!, TypesUsedFromShared("Todo.Api"));
    }

    private static string[] ConsumersOf(string type)
    {
        return SolutionLayers.Ordered
            .Where(layer => layer != Shared)
            .Where(layer => TypesUsedFromShared(layer).Contains(type))
            .Order()
            .ToArray();
    }

    private static string Describe(string[] consumers)
    {
        return consumers.Length == 0 ? "nenhuma" : string.Join(", ", consumers);
    }

    private static IEnumerable<string> PublicTypesOfShared()
    {
        return SolutionLayers.AssemblyOf(Shared)
            .GetExportedTypes()
            .Where(type => !type.IsNested)
            .Select(type => type.FullName!)
            .Order();
    }

    /// <summary>
    /// Nomes dos tipos do <c>Todo.Shared</c> que aparecem nas referências da assembly da camada.
    /// </summary>
    private static IReadOnlySet<string> TypesUsedFromShared(string layer)
    {
        using var file = File.OpenRead(SolutionLayers.AssemblyOf(layer).Location);
        using var assembly = new PEReader(file);

        var metadata = assembly.GetMetadataReader();

        return metadata.TypeReferences
            .Select(metadata.GetTypeReference)
            .Where(type => ComesFromShared(metadata, type))
            .Select(type => $"{metadata.GetString(type.Namespace)}.{metadata.GetString(type.Name)}")
            .ToHashSet(StringComparer.Ordinal);
    }

    private static bool ComesFromShared(MetadataReader metadata, TypeReference type)
    {
        if (type.ResolutionScope.Kind is not HandleKind.AssemblyReference)
        {
            return false;
        }

        var scope = metadata.GetAssemblyReference((AssemblyReferenceHandle)type.ResolutionScope);

        return metadata.GetString(scope.Name).Equals(Shared, StringComparison.Ordinal);
    }
}
