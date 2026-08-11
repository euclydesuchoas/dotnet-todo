using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Todo.Domain.Common;

namespace Todo.Infrastructure.Persistence.Converters;

/// <summary>
/// Grava todo <see cref="DateTime"/> em UTC e devolve os valores lidos já marcados como UTC.
/// </summary>
/// <remarks>
/// Sem isso os três bancos divergem, e divergem de formas diferentes.
///
/// O Npgsql recusa <see cref="DateTime"/> que não seja <see cref="DateTimeKind.Utc"/> ao
/// gravar em <c>timestamp with time zone</c>: lança <see cref="ArgumentException"/> com
/// <c>"only UTC is supported"</c>. Vale para o caminho do EF, em que o tipo do parâmetro vem
/// do mapeamento — em SQL cru sem tipo declarado o Npgsql infere <c>timestamp</c> pelo
/// <see cref="DateTimeKind"/> e o servidor converte em silêncio, o que é outro caminho e não
/// descreve o que esta aplicação faz.
///
/// SQL Server e SQLite ignoram o <see cref="DateTimeKind"/> e o descartam, devolvendo
/// <see cref="DateTimeKind.Unspecified"/> na leitura — ali dois instantes distantes horas
/// entre si são gravados iguais, sem erro nenhum.
///
/// Normalizar na borda do mapeamento deixa o mesmo instante entrando e saindo,
/// independentemente do provider.
///
/// A regra em si mora em <see cref="UtcDateTime.Normalize"/>, no domínio, porque a API e o
/// próprio domínio precisam dela e não enxergam esta camada.
///
/// Valor com <see cref="DateTimeKind.Unspecified"/> é tratado como UTC, e não como hora local:
/// é o que chega do JSON sem offset, e assumir o fuso do servidor deslocaria o instante
/// conforme a máquina que hospeda a aplicação.
/// </remarks>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            value => UtcDateTime.Normalize(value),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}
