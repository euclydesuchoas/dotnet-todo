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
                MapSwaggerIndexRedirect(app);

                app.MapScalarApiReference($"/{DocumentationOptions.RoutePrefix}", options =>
                {
                    options.AddDocuments(ApiVersions.All);
                    options.WithTitle(DocumentTitle);
                });
                break;
        }

        return app;
    }

    /// <summary>
    /// Envia <c>{prefixo}/index.html</c>, a URL que o Swagger deixa no histórico, para a
    /// documentação do Scalar.
    /// </summary>
    /// <remarks>
    /// O Swagger serve em <c>{prefixo}/index.html</c> e redireciona para lá com 301, que os
    /// navegadores cacheiam. Trocando para o Scalar, essa URL cai na sua rota de nome de
    /// documento, que tenta carregar <c>openapi/index.html.json</c> e renderiza um erro no
    /// lugar da documentação. Sendo literal, esta rota tem precedência sobre a rota
    /// parametrizada do Scalar.
    /// <para>
    /// O destino é <c>{prefixo}/</c>, com a barra final, e não <c>{prefixo}</c>: como o 301 do
    /// Swagger fica cacheado justamente para <c>{prefixo}</c>, apontar para lá formaria um laço
    /// de redirecionamento. Com a barra final a URL é outra, e o cache não se aplica.
    /// </para>
    /// </remarks>
    private static void MapSwaggerIndexRedirect(WebApplication app)
    {
        app.MapGet(
            $"/{DocumentationOptions.RoutePrefix}/index.html",
            () => Results.LocalRedirect($"/{DocumentationOptions.RoutePrefix}/"));
    }
}
