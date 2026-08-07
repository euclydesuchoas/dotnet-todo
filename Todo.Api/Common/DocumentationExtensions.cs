using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Todo.Api.Common.Options;
using Todo.Api.Endpoints;

namespace Todo.Api.Common;

public static class DocumentationExtensions
{
    private const string DocumentTitle = "Todo API Documentation";

    /// <summary>
    /// Expõe o documento OpenAPI e a interface de documentação escolhida em
    /// <see cref="DocumentationOptions.Provider"/>. Todos os providers atendem no mesmo
    /// prefixo de rota e consomem os mesmos documentos, um por versão da API.
    /// </summary>
    public static WebApplication UseApiDocumentation(this WebApplication app)
    {
        var provider = app.Services
            .GetRequiredService<IOptions<DocumentationOptions>>()
            .Value
            .Provider;

        if (provider is DocumentationProviderEnum.None)
        {
            return app;
        }

        app.MapOpenApi();

        switch (provider)
        {
            case DocumentationProviderEnum.Swagger:
                app.UseSwaggerUI(options =>
                {
                    foreach (var version in ApiVersions.All)
                    {
                        options.SwaggerEndpoint($"/openapi/{version}.json", $"Todo API {version}");
                    }

                    options.RoutePrefix = DocumentationOptions.RoutePrefix;
                    options.DocumentTitle = DocumentTitle;
                });
                break;

            case DocumentationProviderEnum.Scalar:
                app.MapScalarApiReference($"/{DocumentationOptions.RoutePrefix}", options =>
                {
                    options.AddDocuments(ApiVersions.All);
                    options.WithTitle(DocumentTitle);
                });
                break;
        }

        return app;
    }
}
