namespace Todo.Shared.Time;

/// <summary>
/// Normalização de instantes para UTC.
/// </summary>
/// <remarks>
/// Só as portas chamam isto: a API, ao receber e ao devolver, e o mapeamento de persistência,
/// ao gravar e ao ler. O miolo — casos de uso, validações, domínio — recebe o valor já em UTC
/// e compara direto; normalizar lá dentro seria consertar a falha de uma porta no lugar errado,
/// e obrigaria toda regra nova a lembrar disso.
///
/// Mora no <c>Todo.Shared</c> porque não pertence a nenhuma camada: não é regra de negócio, não
/// orquestra caso de uso e não é detalhe de um provider. É política do projeto sobre como
/// interpretar uma data que chegou sem fuso — decisão de fronteira, usada por bordas que não se
/// enxergam. Ficou no domínio por um tempo só porque era o projeto que todas alcançavam, o que
/// é razão de visibilidade, não de propósito.
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
