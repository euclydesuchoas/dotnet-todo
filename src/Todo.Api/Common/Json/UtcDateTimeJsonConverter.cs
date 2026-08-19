using System.Text.Json;
using System.Text.Json.Serialization;
using Todo.Shared.Time;

namespace Todo.Api.Common.Json;

/// <summary>
/// Faz todo <see cref="DateTime"/> entrar na aplicação em UTC.
/// </summary>
/// <remarks>
/// Sem isso o <see cref="DateTimeKind"/> de um valor desserializado depende do que o cliente
/// escreveu: <c>"…Z"</c> vira <see cref="DateTimeKind.Utc"/>, <c>"…-03:00"</c> vira
/// <see cref="DateTimeKind.Local"/> já convertido para o fuso do servidor, e sem offset vira
/// <see cref="DateTimeKind.Unspecified"/>. Comparação entre dois <see cref="DateTime"/> ignora
/// o <see cref="DateTimeKind"/>, então qualquer regra que compare com
/// <see cref="DateTime.UtcNow"/> erra pela magnitude do offset quando o servidor não roda em
/// UTC — e acerta quando roda, o que esconde o defeito em desenvolvimento.
///
/// Normalizar na desserialização resolve para todo endpoint de uma vez, em vez de exigir que
/// cada regra de validação e cada handler lembre de converter.
///
/// Cobre só corpo JSON: <see cref="DateTime"/> vindo de rota ou query string é vinculado por
/// <c>TryParse</c> e não passa por aqui.
/// </remarks>
public sealed class UtcDateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return UtcDateTime.Normalize(reader.GetDateTime());
    }

    /// <remarks>
    /// O formato <c>"O"</c> sobre um valor em UTC produz o sufixo <c>Z</c>, que é o que o
    /// serializador padrão já faria — está explícito para que a saída não dependa de o valor
    /// ter passado pelo <see cref="Read"/>.
    /// </remarks>
    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(UtcDateTime.Normalize(value).ToString("O"));
    }
}
