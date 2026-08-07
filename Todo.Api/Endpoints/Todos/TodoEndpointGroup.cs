namespace Todo.Api.Endpoints.Todos;

public sealed class TodoEndpointGroup : IEndpointGroup<ApiEndpointGroup>
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent)
    {
        return parent.MapGroup("/v1/todos")
            .WithTags(EndpointTags.Todos);
    }
}
