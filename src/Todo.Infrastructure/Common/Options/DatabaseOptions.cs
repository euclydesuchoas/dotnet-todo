using System.ComponentModel.DataAnnotations;

namespace Todo.Infrastructure.Common.Options;

/// <summary>
/// Define qual banco a aplicação usa e como alcançá-lo.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <remarks>
    /// <see cref="EnumDataTypeAttribute"/>, e não <see cref="RequiredAttribute"/>: um enum
    /// não anulável nunca é nulo, então só a checagem de valor definido rejeita a seção
    /// ausente ou um nome de provider inexistente.
    /// </remarks>
    [EnumDataType(typeof(DatabaseProviderEnum), ErrorMessage = "Database:Provider é obrigatório e deve ser Postgres, SqlServer ou Sqlite.")]
    public DatabaseProviderEnum Provider { get; init; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "Database:ConnectionString é obrigatório.")]
    public string ConnectionString { get; init; } = null!;

    /// <summary>
    /// Cria o banco durante o boot, quando ele ainda não existe.
    /// </summary>
    /// <remarks>
    /// O padrão é desligado, e por motivo mais forte que o de
    /// <see cref="ApplyMigrationsOnStartup"/>: criar banco é privilégio de administração —
    /// <c>CREATEDB</c> no Postgres —, e a aplicação não deveria subir com credencial que o
    /// tenha. Serve a quem está desenvolvendo, para não precisar criar o banco à mão antes
    /// do primeiro boot.
    /// </remarks>
    public bool CreateDatabaseOnStartup { get; init; }

    /// <summary>
    /// Aplica as migrations pendentes durante o boot.
    /// </summary>
    /// <remarks>
    /// O padrão é desligado: em produção o schema costuma ser aplicado por um passo próprio
    /// do deploy, com credenciais próprias, e não pela aplicação subindo.
    /// </remarks>
    public bool ApplyMigrationsOnStartup { get; init; }
}
