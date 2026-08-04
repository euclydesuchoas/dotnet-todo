namespace Todo.Api.Endpoints.Todos;

public sealed class TodoEndpointGroup : IEndpointGroup<TodoEndpointGroup>
{
    public RouteGroupBuilder MapEndpointGroup(IEndpointRouteBuilder app)
    {
        return app.MapGroup("v1/todos")
            .WithTags(EndpointTags.Todos);
    }
}
