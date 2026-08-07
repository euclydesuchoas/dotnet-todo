namespace Todo.Api.Endpoints.V2.Todos;

public sealed class TodoEndpointGroup : IEndpointGroup<V2EndpointGroup>
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent)
    {
        return parent.MapGroup("/todos")
            .WithTags(EndpointTags.Todos);
    }
}
