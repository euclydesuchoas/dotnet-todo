namespace Todo.Infrastructure.Common.Options;

/// <summary>
/// Bancos suportados pela camada de persistência.
/// </summary>
/// <remarks>
/// Não existe membro com valor zero de propósito: assim o padrão do enum é um valor
/// inválido, e a ausência de <c>Database:Provider</c> na configuração derruba a
/// aplicação no boot em vez de eleger um banco por acidente.
/// </remarks>
public enum DatabaseProviderEnum
{
    Postgres = 1,
    SqlServer = 2,
    Sqlite = 3,
}
