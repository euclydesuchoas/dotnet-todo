using System.Reflection;
using Todo.Api.Common.Options;
using Todo.Api.Endpoints;

namespace Todo.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration, Assembly? assembly = null)
    {
        assembly ??= typeof(DependencyInjection).Assembly;

        // Configuração inválida derruba a aplicação no boot, e não na primeira requisição.
        services.AddOptions<DocumentationOptions>()
            .Bind(configuration.GetSection(DocumentationOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        // Um documento por versão: cada endpoint entra no documento cujo nome bate
        // com o WithGroupName aplicado pelo grupo raiz da sua versão.
        foreach (var version in ApiVersions.All)
        {
            services.AddOpenApi(version);
        }

        return services;
    }
}
