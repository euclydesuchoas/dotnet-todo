namespace Todo.Api.Endpoints;

/// <summary>
/// Versões da API. Cada versão gera um documento OpenAPI próprio, listado
/// separadamente no seletor "Select a definition" do Swagger UI.
/// </summary>
/// <remarks>
/// Para adicionar uma versão: inclua a constante aqui, adicione-a em <see cref="All"/>
/// e crie o grupo raiz correspondente (ex.: <c>V2EndpointGroup</c>). O registro do
/// documento OpenAPI e a entrada no Swagger UI passam a existir automaticamente.
/// </remarks>
public static class ApiVersions
{
    public const string V1 = "v1";

    public static readonly IReadOnlyList<string> All = [V1];
}
