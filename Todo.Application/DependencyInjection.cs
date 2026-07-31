using Microsoft.Extensions.DependencyInjection;

namespace Todo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add application services to the container.

        return services;
    }
}
