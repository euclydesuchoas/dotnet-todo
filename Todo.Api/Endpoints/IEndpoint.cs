namespace Todo.Api.Endpoints;

public interface IEndpoint<TEndpointGroup>
    where TEndpointGroup : IEndpointGroup
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
