using Todo.Shared.Temporal;

namespace Todo.Tests.Unit.Common;

public sealed class UtcDateTimeTests
{
    private static readonly DateTime Instant = new(2027, 3, 10, 9, 0, 0);

    [Fact]
    public void Utc_is_kept_as_is()
    {
        var value = DateTime.SpecifyKind(Instant, DateTimeKind.Utc);

        Assert.Equal(value, UtcDateTime.Normalize(value));
        Assert.Equal(DateTimeKind.Utc, UtcDateTime.Normalize(value).Kind);
    }

    [Fact]
    public void Local_is_converted_to_the_same_instant()
    {
        var value = DateTime.SpecifyKind(Instant, DateTimeKind.Local);

        var normalized = UtcDateTime.Normalize(value);

        Assert.Equal(DateTimeKind.Utc, normalized.Kind);
        Assert.Equal(value.ToUniversalTime(), normalized);
    }

    /// <remarks>
    /// É a decisão do projeto: sem offset é UTC, e não hora local. Assumir o fuso do servidor
    /// faria a mesma entrada gravar instantes diferentes conforme a máquina.
    /// </remarks>
    [Fact]
    public void Unspecified_is_taken_as_utc_and_never_shifted()
    {
        var value = DateTime.SpecifyKind(Instant, DateTimeKind.Unspecified);

        var normalized = UtcDateTime.Normalize(value);

        Assert.Equal(DateTimeKind.Utc, normalized.Kind);
        Assert.Equal(Instant.TimeOfDay, normalized.TimeOfDay);
    }

    /// <remarks>
    /// O mesmo instante escrito com offsets diferentes tem que sair idêntico da normalização:
    /// é o que sustenta comparar dois <c>DateTime</c> como comparação de instantes.
    /// </remarks>
    [Theory]
    [InlineData(-3)]
    [InlineData(0)]
    [InlineData(9)]
    public void Every_offset_of_the_same_instant_normalises_to_one_value(int offsetHours)
    {
        var written = new DateTimeOffset(2027, 3, 10, 12 + offsetHours, 0, 0, TimeSpan.FromHours(offsetHours));

        var normalized = UtcDateTime.Normalize(written.UtcDateTime);

        Assert.Equal(new DateTime(2027, 3, 10, 12, 0, 0, DateTimeKind.Utc), normalized);
    }
}
