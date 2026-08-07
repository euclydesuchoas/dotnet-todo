using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Todo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly ??= typeof(DependencyInjection).Assembly;

        // Add infrastructure services to the container.

        return services;
    }
}
