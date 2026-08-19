using Todo.Infrastructure.Common.Options;

namespace Todo.Infrastructure.Common;

/// <summary>
/// Escolhe um valor ou uma ação conforme o banco configurado.
/// </summary>
/// <remarks>
/// Existe para que <see cref="DatabaseProviderEnum"/> seja aberto em um lugar só. Cada
/// provider é um parâmetro obrigatório, então acrescentar um membro ao enum obriga a
/// acrescentar um parâmetro aqui, e isso quebra a compilação de toda chamada existente —
/// nenhum ponto de decisão por banco fica para trás em silêncio.
///
/// É o que um <c>switch</c> por chamada não dá: com descarte ele aceita o membro novo e falha
/// só em runtime; sem descarte ele exige configurar diagnóstico do compilador e suprimir
/// <c>CS8524</c> em cada ponto. Aqui o contrato é a assinatura, e o único <c>switch</c> do
/// projeto sobre este enum é o de baixo.
///
/// Sendo extensão, aparece no IntelliSense do próprio valor: quem for acrescentar um ponto de
/// decisão por banco encontra o método sem precisar saber que ele existe.
///
/// Os ramos são <see cref="Func{TResult}"/> e não valores prontos porque só o escolhido pode
/// executar: em um builder, avaliar os três acumularia os três efeitos.
/// </remarks>
public static class DatabaseProviderMatchExtensions
{
    public static TResult Match<TResult>(
        this DatabaseProviderEnum provider,
        Func<TResult> postgres,
        Func<TResult> sqlServer,
        Func<TResult> sqlite)
    {
        ArgumentNullException.ThrowIfNull(postgres);
        ArgumentNullException.ThrowIfNull(sqlServer);
        ArgumentNullException.ThrowIfNull(sqlite);

        return provider switch
        {
            DatabaseProviderEnum.Postgres => postgres(),
            DatabaseProviderEnum.SqlServer => sqlServer(),
            DatabaseProviderEnum.Sqlite => sqlite(),
            // Alcançável só com um valor fora do enum, que a validação de DatabaseOptions
            // rejeita no boot. Fica como mensagem própria em vez de SwitchExpressionException.
            _ => throw new InvalidOperationException($"Database provider '{provider}' is not supported."),
        };
    }

    public static void Match(
        this DatabaseProviderEnum provider,
        Action postgres,
        Action sqlServer,
        Action sqlite)
    {
        ArgumentNullException.ThrowIfNull(postgres);
        ArgumentNullException.ThrowIfNull(sqlServer);
        ArgumentNullException.ThrowIfNull(sqlite);

        provider.Match<object?>(
            postgres: () => { postgres(); return null; },
            sqlServer: () => { sqlServer(); return null; },
            sqlite: () => { sqlite(); return null; });
    }
}
