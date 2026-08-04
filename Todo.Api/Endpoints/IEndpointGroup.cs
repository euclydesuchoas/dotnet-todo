namespace Todo.Api.Endpoints;

public interface IEndpointGroup
{
    RouteGroupBuilder MapEndpointGroup(IEndpointRouteBuilder app);

    void MapEndpoints(RouteGroupBuilder group, IServiceProvider serviceProvider);
}

public interface IEndpointGroup<TSelf> : IEndpointGroup
    where TSelf : class, IEndpointGroup<TSelf>
{
    // CRTP: TSelf permite resolver IEndpoint<TSelf> via DI sem reflection,
    // pois o tipo concreto do grupo é conhecido em tempo de compilação.
    void IEndpointGroup.MapEndpoints(RouteGroupBuilder group, IServiceProvider serviceProvider)
    {
        var endpoints = serviceProvider.GetServices<IEndpoint<TSelf>>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(group);
        }
    }
}
