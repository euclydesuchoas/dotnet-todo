using Todo.Shared.Text;

namespace Todo.Api.Common.Http;

/// <summary>
/// Faz toda <see cref="string"/> vinculada de rota ou query string entrar na aplicação
/// normalizada.
/// </summary>
/// <remarks>
/// Complementa o <see cref="Json.TextValueJsonConverter"/>, que só alcança corpo JSON. Fora do
/// JSON o vínculo do minimal API entrega o texto exatamente como veio na URL, e o que veio
/// depende do teclado e do sistema de quem digitou: a agulha de uma busca por <c>café</c> chega
/// ora em NFC, ora em NFD, e só uma das duas casa com o que está gravado.
///
/// Vale a mesma ressalva do <see cref="UtcDateTimeEndpointFilter"/>: não alcança
/// <see cref="string"/> aninhada em parâmetro marcado com <c>[AsParameters]</c>, porque ali o
/// argumento é a struct que agrupa os parâmetros, e não o texto dentro dela. Nenhum endpoint do
/// projeto usa <c>[AsParameters]</c> hoje; se algum passar a usar, a normalização precisa
/// acompanhar.
/// </remarks>
public sealed class TextValueEndpointFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var arguments = context.Arguments;

        for (var index = 0; index < arguments.Count; index++)
        {
            // O padrão exclui null por construção, e o Normalize devolve a mesma instância
            // quando o texto já está canônico e sem espaço nas pontas, que é o caso comum.
            if (arguments[index] is string value)
            {
                arguments[index] = TextValue.Normalize(value);
            }
        }

        return next(context);
    }
}
