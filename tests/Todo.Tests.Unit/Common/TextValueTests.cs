using System.Text;
using Todo.Shared.Text;

namespace Todo.Tests.Unit.Common;

/// <remarks>
/// As duas formas do mesmo texto são escritas com escapes, e nunca com o caractere acentuado
/// direto: <c>"café"</c> digitado no editor é uma das duas, e qual delas depende do teclado de
/// quem escreveu o teste. Um teste que não consegue dizer o que está afirmando não afirma nada.
/// </remarks>
public sealed class TextValueTests
{
    /// <summary>café — o acento como um code point só (U+00E9).</summary>
    private const string Composed = "caf\u00e9";

    /// <summary>café — <c>e</c> seguido do acento combinante (U+0301).</summary>
    private const string Decomposed = "cafe\u0301";

    [Fact]
    public void The_two_forms_really_are_different_strings()
    {
        // Sem isto o resto passaria de graça: se as constantes fossem iguais, toda afirmação
        // sobre normalizar uma na outra seria trivialmente verdadeira.
        Assert.NotEqual(Composed, Decomposed);
        Assert.Equal(4, Composed.Length);
        Assert.Equal(5, Decomposed.Length);
    }

    [Fact]
    public void Decomposed_is_composed()
    {
        var normalized = TextValue.Normalize(Decomposed);

        Assert.Equal(Composed, normalized);
        Assert.True(normalized.IsNormalized(NormalizationForm.FormC));
    }

    [Theory]
    [InlineData("  Comprar leite  ", "Comprar leite")]
    [InlineData("\t\n Comprar leite \r\n", "Comprar leite")]
    [InlineData("   ", "")]
    [InlineData("", "")]
    public void Edges_are_trimmed(string value, string expected)
    {
        Assert.Equal(expected, TextValue.Normalize(value));
    }

    [Fact]
    public void Both_at_once()
    {
        Assert.Equal(Composed, TextValue.Normalize($"  {Decomposed}  "));
    }

    /// <remarks>
    /// <c>Assert.Same</c>, e não <c>Assert.Equal</c>: o que se afirma aqui é o atalho — texto já
    /// normalizado não aloca uma cópia. É o caso comum, e é o que torna aceitável aplicar isto a
    /// toda string que entra na API.
    /// </remarks>
    [Fact]
    public void Text_that_is_already_normal_is_returned_untouched()
    {
        Assert.Same(Composed, TextValue.Normalize(Composed));
        Assert.Same("Comprar leite", TextValue.Normalize("Comprar leite"));
    }

    [Fact]
    public void Null_survives()
    {
        // Corpo JSON com null chega assim antes de qualquer validação: normalizar não pode
        // transformar um 400 de campo obrigatório em um 500.
        Assert.Null(TextValue.Normalize(null));
    }

    /// <remarks>
    /// Normalizar o que já foi normalizado tem que devolver o mesmo valor. É o que permite
    /// aplicar a normalização em mais de uma porta sem que a segunda desfaça a primeira.
    /// </remarks>
    [Fact]
    public void Normalising_twice_changes_nothing()
    {
        var once = TextValue.Normalize($"  {Decomposed}  ");

        Assert.Same(once, TextValue.Normalize(once));
    }

    /// <remarks>
    /// A razão prática de tudo isto: sem forma única, um título gravado de um jeito não é
    /// encontrado por uma busca escrita do outro, e os dois são idênticos na tela.
    /// </remarks>
    [Fact]
    public void Normalised_needle_finds_the_normalised_haystack()
    {
        var raw = $"  Tomar {Decomposed} forte  ";

        Assert.DoesNotContain(Composed, raw, StringComparison.Ordinal);
        Assert.Contains(TextValue.Normalize(Composed), TextValue.Normalize(raw), StringComparison.Ordinal);
    }
}
