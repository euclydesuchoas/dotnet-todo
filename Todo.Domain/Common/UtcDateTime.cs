namespace Todo.Domain.Common;

/// <summary>
/// Normalização de instantes para UTC.
/// </summary>
/// <remarks>
/// Mora no domínio porque a mesma regra é usada em três camadas que não enxergam umas às
/// outras: a API normaliza o que chega, o domínio normaliza o que recebe por construção e a
/// persistência normaliza o que grava. Regra duplicada diverge no primeiro ajuste feito em um
/// lugar só.
/// </remarks>
public static class UtcDateTime
{
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
}
