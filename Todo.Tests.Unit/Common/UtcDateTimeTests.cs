using System.Globalization;
using Todo.Domain.Common;

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
    /// As três grafias descrevem o mesmo instante, e é isso que o parse precisa preservar —
    /// é o caminho do vínculo de query string e rota.
    /// </remarks>
    [Theory]
    [InlineData("2027-03-10T12:00:00Z")]
    [InlineData("2027-03-10T09:00:00-03:00")]
    [InlineData("2027-03-10T21:00:00+09:00")]
    public void Parse_maps_every_spelling_to_the_same_instant(string text)
    {
        Assert.True(UtcDateTime.TryParse(text, CultureInfo.InvariantCulture, out var parsed));

        Assert.Equal(DateTimeKind.Utc, parsed.Value.Kind);
        Assert.Equal(new DateTime(2027, 3, 10, 12, 0, 0, DateTimeKind.Utc), parsed.Value);
    }

    [Fact]
    public void Parse_without_offset_takes_the_text_as_utc()
    {
        Assert.True(UtcDateTime.TryParse("2027-03-10T12:00:00", CultureInfo.InvariantCulture, out var parsed));

        Assert.Equal(new DateTime(2027, 3, 10, 12, 0, 0, DateTimeKind.Utc), parsed.Value);
    }

    [Fact]
    public void Parse_rejects_text_that_is_not_a_date()
    {
        Assert.False(UtcDateTime.TryParse("nope", CultureInfo.InvariantCulture, out _));
    }

    [Fact]
    public void Comparison_is_by_instant()
    {
        var utc = new UtcDateTime(new DateTime(2027, 3, 10, 12, 0, 0, DateTimeKind.Utc));
        var sameInstantAsLocal = new UtcDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(new DateTime(2027, 3, 10, 12, 0, 0), TimeZoneInfo.Local) is var local
                ? DateTime.SpecifyKind(local, DateTimeKind.Local)
                : default);

        Assert.Equal(utc, sameInstantAsLocal);
        Assert.False(utc < sameInstantAsLocal);
        Assert.False(utc > sameInstantAsLocal);
    }
}
