using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Todo.Shared.Text;

/// <summary>
/// Normalização de texto vindo de fora.
/// </summary>
/// <remarks>
/// Hoje só as portas da API chamam isto: o conversor de JSON, ao desserializar o corpo, e o
/// filtro de endpoint, sobre o que veio de rota e query string. O miolo — casos de uso,
/// validações, domínio — recebe o texto já normalizado, exatamente como recebe as datas já em
/// UTC.
///
/// Mora aqui, e não no <c>Todo.Api</c>, pela mesma razão do <see cref="Time.UtcDateTime"/>: é
/// política de fronteira sobre como interpretar um valor que chegou de fora, não depende de
/// tecnologia nenhuma e não pertence a camada alguma. Estar no compartilhado é o que permite
/// que qualquer camada a use sem que a regra precise mudar de lugar antes.
///
/// A diferença em relação ao <see cref="Time.UtcDateTime"/> é só o número de consumidores:
/// aquele é usado pela borda de HTTP e pela de persistência; este, por enquanto, só pela
/// primeira. Isso não muda o que ele é, e é por isso que ele está aqui.
/// </remarks>
public static class TextValue
{
    /// <summary>
    /// Devolve o mesmo texto na forma canônica NFC e sem espaço nas pontas.
    /// </summary>
    /// <remarks>
    /// A canonização resolve um defeito que ninguém consegue ver na tela. Um caractere acentuado
    /// tem duas representações canônicas — <c>é</c> é tanto <c>U+00E9</c> (NFC, um code point)
    /// quanto <c>e</c> seguido de <c>U+0301</c> (NFD, dois) —, e qual delas chega depende do
    /// teclado e do sistema de quem digitou. Sem uma forma única, um título gravado em uma forma
    /// não é encontrado por uma busca escrita na outra, e o mesmo texto conta um número
    /// diferente de caracteres na validação de tamanho.
    ///
    /// Aparar resolve outro: sem isso, <c>"Comprar leite"</c> e <c>"  Comprar leite  "</c> viram
    /// duas linhas distintas e idênticas na tela, e um título no limite de tamanho é rejeitado
    /// por causa dos espaços.
    ///
    /// Aparar vale para toda string que entra porque nenhum campo desta API guarda espaço nas
    /// pontas como conteúdo. No dia em que entrar um — texto com formatação preservada, senha —,
    /// esta decisão deixa de poder ser tomada em bloco aqui, e passa a ser por campo. Canonizar
    /// continua valendo para todos: não descarta nada, só escolhe uma codificação.
    ///
    /// Testar a forma antes evita alocar no caso comum, em que o texto já chega em NFC — mesma
    /// razão do <c>Kind: not DateTimeKind.Utc</c> no filtro de datas.
    /// </remarks>
    [return: NotNullIfNotNull(nameof(value))]
    public static string? Normalize(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var canonical = value.IsNormalized(NormalizationForm.FormC)
            ? value
            : value.Normalize(NormalizationForm.FormC);

        return canonical.Trim();
    }
}
