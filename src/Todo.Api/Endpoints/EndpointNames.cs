namespace Todo.Api.Endpoints;

/// <summary>
/// Nomes de rota, usados para geração de links e como <c>operationId</c> no OpenAPI.
/// Precisam ser únicos em toda a aplicação, por isso são segregados por versão.
/// </summary>
public static class EndpointNames
{
    public static class V1
    {
        public const string CreateTodoItem = "CreateTodoItemV1";

        public const string GetTodoItemById = "GetTodoItemByIdV1";

        public const string GetTodoItems = "GetTodoItemsV1";
    }

    public static class V2
    {
        public const string CreateTodoItem = "CreateTodoItemV2";

        public const string GetTodoItemById = "GetTodoItemByIdV2";
    }
}
