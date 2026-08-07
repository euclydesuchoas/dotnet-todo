namespace Todo.Api.Endpoints;

/// <summary>
/// Origem única dos segmentos de rota, usados pelos grupos ao chamar <c>MapGroup</c>.
/// </summary>
/// <remarks>
/// São constantes de compilação. Aqui ficam apenas os trechos relativos: caminhos
/// absolutos não são montados à mão, e sim gerados a partir da tabela de rotas
/// (<c>CreatedAtRoute</c> / <c>LinkGenerator</c>), tendo o nome do endpoint como
/// referência.
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
}
