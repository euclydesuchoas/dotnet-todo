namespace Todo.Api.Endpoints.V1.TodoItems;

public sealed class TodoItemEndpointGroup : IEndpointGroup<V1EndpointGroup>
{
    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder parent)
    {
        return parent.MapGroup("/todo-items")
            .WithTags(EndpointTags.TodoItems);
    }
}
