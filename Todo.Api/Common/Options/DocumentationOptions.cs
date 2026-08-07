namespace Todo.Api.Common.Options;

/// <summary>
/// Controla qual interface de documentação é exposta, e se alguma é.
/// </summary>
public sealed class DocumentationOptions
{
    public const string SectionName = "Documentation";

    /// <summary>
    /// Prefixo de rota da documentação, compartilhado por todos os providers.
    /// </summary>
    /// <remarks>
    /// É constante de propósito: a URL não deve mudar quando o provider muda, senão
    /// links e bookmarks quebram a cada troca — que é justamente o que a opção
    /// <see cref="Provider"/> pretende tornar um detalhe interno.
    /// </remarks>
    public const string RoutePrefix = "documentation";

    /// <summary>
    /// O padrão é <see cref="DocumentationProviderEnum.None"/>, de modo que a ausência
    /// da seção na configuração resulte em documentação desligada.
    /// </summary>
    public DocumentationProviderEnum Provider { get; init; } = DocumentationProviderEnum.None;
}
