using System.Globalization;

namespace Todo.Domain.Common;

/// <summary>
/// Instante em UTC. Normaliza na construção e no parse.
/// </summary>
/// <remarks>
/// Mora no domínio porque a mesma regra é usada em três camadas que não enxergam umas às
/// outras: a API normaliza o que chega, o domínio normaliza o que recebe por construção e a
/// persistência normaliza o que grava. Regra duplicada diverge no primeiro ajuste feito em um
/// lugar só.
///
/// O tipo existe, e não só o método <see cref="Normalize"/>, por causa do vínculo de query
/// string e rota: ali não passa <c>System.Text.Json</c>, e sim
/// <see cref="IParsable{TSelf}.TryParse"/> do tipo declarado no parâmetro. Declarar
/// <see cref="DateTime"/> entrega um valor cujo <see cref="DateTimeKind"/> depende do que o
/// cliente escreveu; declarar este tipo entrega o instante já normalizado, e não há como
/// esquecer de converter depois.
/// </remarks>
public readonly struct UtcDateTime : IParsable<UtcDateTime>, IEquatable<UtcDateTime>, IComparable<UtcDateTime>
{
    public UtcDateTime(DateTime value)
    {
        Value = Normalize(value);
    }

    public DateTime Value { get; }

    /// <summary>
    /// Devolve o mesmo instante com <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="DateTimeKind.Unspecified"/> é tratado como UTC, e não como hora local: é o
    /// que chega de uma entrada sem offset, e assumir o fuso do servidor deslocaria o instante
    /// conforme a máquina que hospeda a aplicação — a mesma requisição gravaria valores
    /// diferentes em dev e em produção.
    /// </remarks>
    public static DateTime Normalize(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };
    }

    /// <remarks>
    /// <see cref="DateTimeStyles.RoundtripKind"/> é o que preserva o <see cref="DateTimeKind"/>
    /// escrito na entrada. Sem ele, o <c>TryParse</c> padrão converte <c>"…Z"</c> para hora
    /// local e devolve <see cref="DateTimeKind.Local"/> — a informação de que a entrada já
    /// estava em UTC se perde antes de <see cref="Normalize"/> poder usá-la.
    /// </remarks>
    public static bool TryParse(string? s, IFormatProvider? provider, out UtcDateTime result)
    {
        if (DateTime.TryParse(s, provider ?? CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var value))
        {
            result = new UtcDateTime(value);

            return true;
        }

        result = default;

        return false;
    }

    public static UtcDateTime Parse(string s, IFormatProvider? provider)
    {
        return TryParse(s, provider, out var result)
            ? result
            : throw new FormatException($"'{s}' is not a valid date and time.");
    }

    public static implicit operator DateTime(UtcDateTime value) => value.Value;

    public static implicit operator UtcDateTime(DateTime value) => new(value);

    public static bool operator ==(UtcDateTime left, UtcDateTime right) => left.Equals(right);

    public static bool operator !=(UtcDateTime left, UtcDateTime right) => !left.Equals(right);

    public static bool operator <(UtcDateTime left, UtcDateTime right) => left.CompareTo(right) < 0;

    public static bool operator >(UtcDateTime left, UtcDateTime right) => left.CompareTo(right) > 0;

    public static bool operator <=(UtcDateTime left, UtcDateTime right) => left.CompareTo(right) <= 0;

    public static bool operator >=(UtcDateTime left, UtcDateTime right) => left.CompareTo(right) >= 0;

    public bool Equals(UtcDateTime other) => Value.Equals(other.Value);

    public override bool Equals(object? obj) => obj is UtcDateTime other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public int CompareTo(UtcDateTime other) => Value.CompareTo(other.Value);

    public override string ToString() => Value.ToString("O");
}
