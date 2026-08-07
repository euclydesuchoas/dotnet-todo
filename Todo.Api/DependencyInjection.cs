using System.Reflection;

namespace Todo.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly ??= typeof(DependencyInjection).Assembly;

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        services.AddOpenApi();

        return services;
    }
}
