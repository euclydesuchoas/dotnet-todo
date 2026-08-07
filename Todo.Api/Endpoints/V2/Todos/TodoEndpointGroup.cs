namespace Todo.Api.Endpoints.V2.Todos;

public sealed class TodoEndpointGroup : IEndpointGroup<V2EndpointGroup>
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent)
    {
        return parent.MapGroup(EndpointRoutes.Segments.Todos)
            .WithTags(EndpointTags.Todos);
    }
}
