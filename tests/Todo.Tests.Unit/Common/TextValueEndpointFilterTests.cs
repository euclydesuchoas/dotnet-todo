using Microsoft.AspNetCore.Http;
using System.Text;
using Todo.Api.Common.Http;

namespace Todo.Tests.Unit.Common;

/// <summary>
/// Cobre o que o filtro faz com os argumentos já vinculados de rota e query string.
/// </summary>
/// <remarks>
/// Mesma divisão do <see cref="UtcDateTimeEndpointFilterTests"/>: o vínculo é do ASP.NET Core e
/// não é reexecutado aqui. O que se afirma é a garantia sobre os argumentos prontos.
/// </remarks>
public sealed class TextValueEndpointFilterTests
{
    private const string Composed = "caf\u00e9";

    private const string Decomposed = "cafe\u0301";

    private static async Task<IList<object?>> InvokeAsync(params object?[] arguments)
    {
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), arguments);

        await new TextValueEndpointFilter().InvokeAsync(context, _ => ValueTask.FromResult<object?>(null));

        return context.Arguments;
    }

    /// <remarks>
    /// A agulha de uma busca chega por aqui. Se ela não for normalizada, a busca por um termo
    /// acentuado deixa de encontrar o que foi gravado pela outra porta — e as duas grafias são
    /// idênticas na tela.
    /// </remarks>
    [Fact]
    public async Task Decomposed_argument_is_composed()
    {
        var arguments = await InvokeAsync(Decomposed);

        var normalized = Assert.IsType<string>(arguments[0]);

        Assert.Equal(Composed, normalized);
        Assert.True(normalized.IsNormalized(NormalizationForm.FormC));
    }

    [Fact]
    public async Task Whitespace_is_trimmed()
    {
        var arguments = await InvokeAsync("  Comprar leite  ");

        Assert.Equal("Comprar leite", arguments[0]);
    }

    [Fact]
    public async Task Null_and_other_types_are_left_alone()
    {
        var instant = new DateTime(2027, 3, 10, 9, 0, 0, DateTimeKind.Utc);

        var arguments = await InvokeAsync(null, 42, instant);

        Assert.Null(arguments[0]);
        Assert.Equal(42, arguments[1]);
        Assert.Equal(instant, arguments[2]);
    }
}
