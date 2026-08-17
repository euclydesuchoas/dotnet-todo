using Microsoft.AspNetCore.Http;
using Todo.Api.Common.Http;

namespace Todo.Tests.Unit.Common;

/// <summary>
/// Cobre o que o filtro faz com os argumentos já vinculados.
/// </summary>
/// <remarks>
/// O vínculo em si é do ASP.NET Core e não é reexecutado aqui: o filtro recebe os argumentos
/// prontos, e é sobre eles que a garantia vale. O caso que importa é o
/// <see cref="DateTimeKind.Unspecified"/>, que é o que o binder de query string deixa passar
/// quando a entrada não traz offset.
/// </remarks>
public sealed class UtcDateTimeEndpointFilterTests
{
    private static readonly DateTime Instant = new(2027, 3, 10, 9, 0, 0);

    // DefaultEndpointFilterInvocationContext, e não EndpointFilterInvocationContext.Create: as
    // sobrecargas de Create são genéricas por aridade, e um object?[] casa com Create<T> como
    // argumento único em vez de virar a lista de argumentos.
    private static async Task<IList<object?>> InvokeAsync(params object?[] arguments)
    {
        var context = new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), arguments);

        await new UtcDateTimeEndpointFilter().InvokeAsync(context, _ => ValueTask.FromResult<object?>(null));

        return context.Arguments;
    }

    [Fact]
    public async Task Unspecified_argument_is_marked_as_utc_without_shifting()
    {
        var arguments = await InvokeAsync(DateTime.SpecifyKind(Instant, DateTimeKind.Unspecified));

        var normalized = Assert.IsType<DateTime>(arguments[0]);

        Assert.Equal(DateTimeKind.Utc, normalized.Kind);
        Assert.Equal(Instant.TimeOfDay, normalized.TimeOfDay);
    }

    [Fact]
    public async Task Local_argument_is_converted_to_the_same_instant()
    {
        var local = DateTime.SpecifyKind(Instant, DateTimeKind.Local);

        var arguments = await InvokeAsync(local);

        var normalized = Assert.IsType<DateTime>(arguments[0]);

        Assert.Equal(DateTimeKind.Utc, normalized.Kind);
        Assert.Equal(local.ToUniversalTime(), normalized);
    }

    /// <remarks>
    /// <c>DateTime?</c> é empacotado como <c>DateTime</c>, então percorre o mesmo caminho —
    /// e o nulo não pode virar um valor.
    /// </remarks>
    [Fact]
    public async Task Nullable_argument_is_normalised_and_null_is_left_alone()
    {
        DateTime? filled = DateTime.SpecifyKind(Instant, DateTimeKind.Unspecified);
        DateTime? empty = null;

        var arguments = await InvokeAsync(filled, empty);

        Assert.Equal(DateTimeKind.Utc, Assert.IsType<DateTime>(arguments[0]).Kind);
        Assert.Null(arguments[1]);
    }

    [Fact]
    public async Task Arguments_that_are_not_dates_are_left_alone()
    {
        var arguments = await InvokeAsync("texto", 42, null);

        Assert.Equal("texto", arguments[0]);
        Assert.Equal(42, arguments[1]);
        Assert.Null(arguments[2]);
    }
}
