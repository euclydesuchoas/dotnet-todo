using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using Todo.Shared.Time;

namespace Todo.Tests.Architecture;

/// <summary>
/// Mantém no <c>Todo.Shared</c> só o que é compartilhado de verdade.
/// </summary>
/// <remarks>
/// O compartilhado é o único projeto definido pelo uso, e não por um assunto, o que o torna o
/// candidato natural a virar depósito: qualquer utilitário cabe lá se o critério for "é
/// conveniente, e todo mundo alcança". O critério é outro — fica no compartilhado o tipo que
/// duas camadas que não se enxergam usam. Um consumidor só significa que o tipo pertence a ele,
/// e não à base, e é para lá que deve voltar.
///
/// A contagem sai da tabela de <c>TypeRef</c> de cada assembly, e não de reflexão sobre os tipos
/// carregados no processo: o <c>TypeRef</c> registra a dependência mesmo quando o uso está no
/// corpo de um método que nenhum teste executa.
/// </remarks>
public sealed class SharedContentTests
{
    private const string Shared = "Todo.Shared";

    private const int RequiredConsumers = 2;

    public static TheoryData<string> SharedTypes => [.. PublicTypesOfShared()];

    [Theory]
    [MemberData(nameof(SharedTypes))]
    public void Shared_type_is_used_by_at_least_two_layers(string type)
    {
        var consumers = ConsumersOf(type);

        Assert.True(
            consumers.Length >= RequiredConsumers,
            $"{type} está no {Shared}, mas é usado por {consumers.Length} camada(s): {Describe(consumers)}. "
            + $"O compartilhado guarda o que {RequiredConsumers} camadas que não se enxergam usam; "
            + "com menos que isso, o tipo pertence à camada que o usa.");
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
