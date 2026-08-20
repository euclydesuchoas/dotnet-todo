using System.Text;
using System.Text.Json;
using Todo.Api.Common.Json;

namespace Todo.Tests.Unit.Common;

public sealed class TextValueJsonConverterTests
{
    private const string Composed = "caf\u00e9";

    private const string Decomposed = "cafe\u0301";

    // JsonSerializerDefaults.Web, e não o padrão cru: é o que o ConfigureHttpJsonOptions usa, e
    // é o que decide a caixa dos nomes de propriedade. Um teste sobre outro padrão afirmaria
    // sobre um serializador que a aplicação não tem.
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new TextValueJsonConverter() },
    };

    private sealed record Payload(string Title, string? Description);

    [Fact]
    public void Decomposed_body_is_normalised_on_the_way_in()
    {
        var payload = JsonSerializer.Deserialize<Payload>(
            $$"""{"title":"{{Decomposed}}","description":null}""",
            Options);

        Assert.NotNull(payload);
        Assert.Equal(Composed, payload.Title);
        Assert.True(payload.Title.IsNormalized(NormalizationForm.FormC));
    }

    /// <remarks>
    /// O nulo é resolvido pelo serializador antes de chegar ao conversor — <c>HandleNull</c> é
    /// <c>false</c> para tipo de referência. Está afirmado porque o conversor declara
    /// <c>string</c> não anulável no <c>Read</c>, e essa assinatura só é segura por causa disso.
    /// </remarks>
    [Fact]
    public void Null_does_not_reach_the_converter()
    {
        var payload = JsonSerializer.Deserialize<Payload>(
            """{"title":"a","description":null}""",
            Options);

        Assert.Null(payload!.Description);
    }

    /// <remarks>
    /// Canonizar na saída mascararia o que está gravado: se algo escapou da normalização na
    /// entrada, tem que aparecer na resposta.
    /// </remarks>
    [Fact]
    public void Output_is_written_as_it_is()
    {
        var json = JsonSerializer.Serialize(new Payload(Decomposed, null), Options);

        // Relido sem o conversor: o STJ escapa não-ASCII na saída, então comparar o texto cru
        // do JSON afirmaria sobre o escape, e não sobre o valor.
        var reread = JsonSerializer.Deserialize<Payload>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Equal(Decomposed, reread!.Title);
    }

    [Fact]
    public void Whitespace_is_trimmed()
    {
        var payload = JsonSerializer.Deserialize<Payload>(
            """{"title":"  Comprar leite  ","description":null}""",
            Options);

        Assert.Equal("Comprar leite", payload!.Title);
    }

    /// <remarks>
    /// Sem as sobrecargas de nome de propriedade no conversor, a implementação padrão de
    /// <c>JsonConverter&lt;T&gt;</c> lança <c>NotSupportedException</c>, e qualquer resposta com
    /// um dicionário de chave string passaria a falhar só porque este conversor foi registrado.
    /// O <c>ProblemDetails</c> do ASP.NET Core tem um.
    /// </remarks>
    [Fact]
    public void Dictionary_keys_still_work()
    {
        var value = new Dictionary<string, int> { ["um"] = 1 };

        var json = JsonSerializer.Serialize(value, Options);

        Assert.Equal("""{"um":1}""", json);
        Assert.Equal(value, JsonSerializer.Deserialize<Dictionary<string, int>>(json, Options));
    }
}
