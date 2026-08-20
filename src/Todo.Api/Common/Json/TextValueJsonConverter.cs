using System.Text.Json;
using System.Text.Json.Serialization;
using Todo.Shared.Text;

namespace Todo.Api.Common.Json;

/// <summary>
/// Faz toda <see cref="string"/> de corpo JSON entrar na aplicação normalizada.
/// </summary>
/// <remarks>
/// Sem isso a forma de um texto acentuado depende de quem o digitou: o mesmo <c>café</c> chega
/// ora como <c>caf</c> + <c>U+00E9</c>, ora como <c>cafe</c> + <c>U+0301</c>. Renderizam igual e
/// comparam diferente, então um título gravado em uma forma não é encontrado por uma busca
/// escrita na outra — e ninguém consegue ver a diferença na tela para reportar o defeito.
///
/// Normalizar na desserialização resolve para todo endpoint de uma vez, em vez de exigir que
/// cada validação e cada handler lembre de converter. É o mesmo desenho do
/// <see cref="UtcDateTimeJsonConverter"/>, e pela mesma razão.
///
/// Cobre só corpo JSON: <see cref="string"/> vinda de rota ou query string é vinculada sem
/// passar por aqui, e quem cobre esse caminho é o <see cref="Http.TextValueEndpointFilter"/>.
/// </remarks>
public sealed class TextValueJsonConverter : JsonConverter<string>
{
    /// <remarks>
    /// Não é chamado para <c>null</c>: <see cref="JsonConverter{T}.HandleNull"/> é <c>false</c>
    /// para tipo de referência, então o serializador resolve o nulo sozinho. Um campo
    /// obrigatório ausente continua sendo rejeitado pela validação, e não aqui.
    /// </remarks>
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return TextValue.Normalize(reader.GetString()!);
    }

    /// <remarks>
    /// Escreve o valor como está. Normalizar na saída mascararia o que foi gravado: se algo
    /// escapou da normalização na entrada, o defeito tem que aparecer na resposta em vez de ser
    /// consertado no caminho de volta.
    /// </remarks>
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }

    /// <remarks>
    /// As duas sobrecargas de nome de propriedade existem porque a implementação padrão de
    /// <see cref="JsonConverter{T}"/> lança <see cref="NotSupportedException"/>: sem elas, um
    /// <c>Dictionary&lt;string, T&gt;</c> em qualquer resposta passaria a falhar só por este
    /// conversor estar registrado — o <c>ProblemDetails</c> do ASP.NET Core tem um. Chave de
    /// dicionário é identificador, e não texto de usuário, então segue sem alteração.
    /// </remarks>
    public override string ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString()!;
    }

    /// <inheritdoc cref="ReadAsPropertyName"/>
    public override void WriteAsPropertyName(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value);
    }
}
