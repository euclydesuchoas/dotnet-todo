using Todo.Domain.Common;

namespace Todo.Api.Common.Http;

/// <summary>
/// Faz todo <see cref="DateTime"/> vinculado de rota ou query string entrar na aplicação em UTC.
/// </summary>
/// <remarks>
/// Complementa o <see cref="Json.UtcDateTimeJsonConverter"/>, que só alcança corpo JSON. Fora do
/// JSON quem converte o texto é o vínculo do minimal API, e o que ele deixa passar é a entrada
/// sem offset: o binder usa <c>DateTimeStyles.AdjustToUniversal</c> com
/// <c>CultureInfo.InvariantCulture</c>, então <c>"…Z"</c> e <c>"…-03:00"</c> já chegam em
/// <see cref="DateTimeKind.Utc"/> — o segundo com o instante convertido —, enquanto <c>"…"</c>
/// sem offset chega <see cref="DateTimeKind.Unspecified"/>. Comparação entre dois
/// <see cref="DateTime"/> ignora o <see cref="DateTimeKind"/>, então esse último valor seguiria
/// adiante como se fosse UTC sem nunca ter sido marcado como tal, e a persistência o gravaria
/// com o tratamento que cada banco dá a data sem fuso.
///
/// O filtro roda depois do vínculo e reescreve os argumentos, o que mantém
/// <see cref="DateTime"/> na assinatura do endpoint. Declarar um tipo próprio no parâmetro
/// também normalizaria, mas ao custo do documento OpenAPI: o schema de um tipo customizado sai
/// como <c>string</c> pura, sem o <c>format: date-time</c> que o <see cref="DateTime"/> produz,
/// e tanto a UI de documentação quanto os geradores de client passam a tratar a data como texto.
///
/// Não alcança <see cref="DateTime"/> aninhado em parâmetro marcado com <c>[AsParameters]</c>:
/// ali o argumento é a struct que agrupa os filtros, e não a data dentro dela. Se algum endpoint
/// passar a agrupar parâmetros assim, a normalização precisa acompanhar.
/// </remarks>
public sealed class UtcDateTimeEndpointFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var arguments = context.Arguments;

        for (var index = 0; index < arguments.Count; index++)
        {
            // Testar o Kind evita reempacotar o caso comum, em que o valor já chega em UTC.
            // DateTime? entra por aqui do mesmo jeito: Nullable<T> é empacotado como T.
            if (arguments[index] is DateTime { Kind: not DateTimeKind.Utc } value)
            {
                arguments[index] = UtcDateTime.Normalize(value);
            }
        }

        return next(context);
    }
}
