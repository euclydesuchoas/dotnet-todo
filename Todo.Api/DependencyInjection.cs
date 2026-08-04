using Todo.Api.Endpoints;

namespace Todo.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();

        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes.AssignableToAny(typeof(IEndpointGroup), typeof(IEndpoint<>)))
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        return services;
    }

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        var rootEndpointGroup = app
            .MapGroup("/api");

        var serviceProvider = app.ServiceProvider;
        var endpointGroups = serviceProvider.GetServices<IEndpointGroup>();

        foreach (var endpointGroup in endpointGroups)
        {
            var mappedEndpointGroup = endpointGroup.MapEndpointGroup(rootEndpointGroup);
            endpointGroup.MapEndpoints(mappedEndpointGroup, serviceProvider);
        }

        return app;
    }
}
