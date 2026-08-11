namespace Todo.Api.Endpoints;

/// <summary>
/// Nomes de rota, usados para geração de links e como <c>operationId</c> no OpenAPI.
/// Precisam ser únicos em toda a aplicação, por isso são segregados por versão.
/// </summary>
public static class EndpointNames
{
    public static class V1
    {
        public const string CreateTodo = "CreateTodoV1";

        public const string GetTodoById = "GetTodoByIdV1";

        public const string GetTodos = "GetTodosV1";
    }

    public static class V2
    {
        public const string CreateTodo = "CreateTodoV2";

        public const string GetTodoById = "GetTodoByIdV2";
    }
}
