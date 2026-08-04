using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Messaging;

namespace Todo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblies(typeof(DependencyInjection).Assembly)
            .AddClasses(classes => classes.AssignableToAny(typeof(IServiceHandler<>), typeof(IServiceHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.TryDecorate(typeof(IServiceHandler<>), typeof(ServiceHandler<>));
        services.TryDecorate(typeof(IServiceHandler<,>), typeof(ServiceHandler<,>));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
