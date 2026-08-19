using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Messaging;

namespace Todo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, Assembly? assembly = null)
    {
        assembly ??= typeof(DependencyInjection).Assembly;

        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableToAny(typeof(IServiceHandler<>), typeof(IServiceHandler<,>)), publicOnly: false)
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.TryDecorate(typeof(IServiceHandler<>), typeof(ServiceHandler<>));
        services.TryDecorate(typeof(IServiceHandler<,>), typeof(ServiceHandler<,>));

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // O TimeProvider não vem registrado pelo host — nem pelo WebApplication, nem por um
        // ServiceCollection vazio —, então quem depende dele precisa registrá-lo. Fica aqui, e
        // não na composição da API, para a camada se sustentar sozinha em teste e em qualquer
        // outro hospedeiro. TryAdd para o hospedeiro poder trocar o relógio sem editar isto.
        services.TryAddSingleton(TimeProvider.System);

        return services;
    }
}
