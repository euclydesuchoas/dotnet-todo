namespace Todo.Api.Endpoints;

/// <summary>
/// Origem única dos caminhos de rota.
/// </summary>
/// <remarks>
/// <see cref="Segments"/> contém os trechos relativos, que são o que os grupos passam
/// para <c>MapGroup</c>. As constantes de caminho absoluto são compostas a partir desses
/// mesmos trechos, para quem precisa do path inteiro (header <c>Location</c>, geração de
/// links). Alterar um segmento propaga para o grupo e para os caminhos absolutos.
/// <para>
/// São constantes de compilação: a composição é resolvida pelo compilador, sem custo
/// em tempo de execução.
/// </para>
/// </remarks>
public static class EndpointRoutes
{
    public static class Segments
    {
        public const string Api = "/api";

        public const string V1 = "/" + ApiVersions.V1;

        public const string V2 = "/" + ApiVersions.V2;

        public const string Todos = "/todos";
    }

    public const string V1Todos = Segments.Api + Segments.V1 + Segments.Todos;

    public const string V2Todos = Segments.Api + Segments.V2 + Segments.Todos;
}
