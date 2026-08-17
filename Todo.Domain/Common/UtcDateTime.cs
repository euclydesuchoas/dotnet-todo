namespace Todo.Domain.Common;

/// <summary>
/// Normalização de instantes para UTC.
/// </summary>
/// <remarks>
/// Só as portas chamam isto: a API, ao receber e ao devolver, e o mapeamento de persistência,
/// ao gravar e ao ler. O miolo — casos de uso, validações, domínio — recebe o valor já em UTC
/// e compara direto; normalizar lá dentro seria consertar a falha de uma porta no lugar errado,
/// e obrigaria toda regra nova a lembrar disso.
///
/// Mora no projeto mais interno porque as duas portas precisam da mesma regra sem que uma
/// dependa da outra, e o domínio é o único que ambas enxergam. Não é regra de negócio, e sim
/// política de fuso do projeto, num lugar que todos alcançam — se a solução ganhar um projeto
/// compartilhado próprio, o endereço natural passa a ser ele.
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
