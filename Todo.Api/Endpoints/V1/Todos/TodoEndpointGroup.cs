namespace Todo.Api.Endpoints.V1.Todos;

public sealed class TodoEndpointGroup : IEndpointGroup<V1EndpointGroup>
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent)
    {
        return parent.MapGroup(EndpointRoutes.Segments.Todos)
            .WithTags(EndpointTags.Todos);
    }
}
