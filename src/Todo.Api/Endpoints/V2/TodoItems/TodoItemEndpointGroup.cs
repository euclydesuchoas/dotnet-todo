namespace Todo.Api.Endpoints.V2.TodoItems;

public sealed class TodoItemEndpointGroup : IEndpointGroup<V2EndpointGroup>
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent)
    {
        return parent.MapGroup("/todo-items")
            .WithTags(EndpointTags.TodoItems);
    }
}
