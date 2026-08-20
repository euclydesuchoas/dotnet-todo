using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Todo.Api;
using Todo.Api.Endpoints;

namespace Todo.Tests.Integration.Api;

/// <summary>
/// Verifica que o corpo JSON é uma porta: texto entra canônico e sem espaço nas pontas.
/// </summary>
/// <remarks>
/// Esta é a porta que alimenta os casos de uso de escrita, e nada no miolo normaliza texto — o
/// validator mede o tamanho e o domínio grava o que recebeu. Se o <c>TextValueJsonConverter</c>
/// deixar de ser registrado, ou parar de normalizar, a falha aparece aqui e não como linha
/// duplicada no banco meses depois.
///
/// O host chama <see cref="DependencyInjection.AddApi"/> de propósito: o que está sob teste é o
/// registro do converter pelo código de produção, e não o converter isolado.
///
/// As duas formas do mesmo texto são escritas com escapes, e nunca com o caractere acentuado
/// direto: qual delas o editor produziria depende do teclado de quem escreveu o teste.
/// </remarks>
public sealed class TextValueJsonBodyTests : IAsyncLifetime
{
    /// <summary>café — o acento como um code point só (U+00E9).</summary>
    private const string Composed = "caf\u00e9";

    /// <summary>café — <c>e</c> seguido do acento combinante (U+0301).</summary>
    private const string Decomposed = "cafe\u0301";

    private sealed record ProbeRequest(string Title, string? Optional);

    private sealed record ProbeResponse(string Title, int Length, string? Optional, string ServerSide);

    private WebApplication app = null!;

    private HttpClient client = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.WebHost.UseTestServer();
        builder.Services.AddApi(new ConfigurationBuilder().Build());

        app = builder.Build();

        // O comprimento vai na resposta porque é o que distingue as duas formas do mesmo texto:
        // elas são idênticas quando renderizadas.
        new ApiEndpointGroup().MapGroup(app).MapPost("/probe", (ProbeRequest body) =>
            new ProbeResponse(body.Title, body.Title.Length, body.Optional, Decomposed));

        await app.StartAsync(TestContext.Current.CancellationToken);

        client = app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        client.Dispose();

        await app.DisposeAsync();
    }

    /// <remarks>
    /// Envia o JSON como texto cru: se o teste serializasse com as próprias opções, mediria o
    /// cliente e não o servidor.
    /// </remarks>
    private async Task<ProbeResponse> PostAsync(string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/probe", content, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<ProbeResponse>(TestContext.Current.CancellationToken))!;
    }

    /// <remarks>
    /// As duas grafias descrevem o mesmo texto. Sem uma forma única, um título gravado por uma
    /// delas não é encontrado por uma busca escrita na outra.
    /// </remarks>
    [Theory]
    [InlineData(Composed)]
    [InlineData(Decomposed)]
    public async Task Every_form_in_the_body_arrives_as_one_value(string written)
    {
        var probe = await PostAsync($$"""{"title":"Tomar {{written}} forte"}""");

        Assert.Equal($"Tomar {Composed} forte", probe.Title);
        Assert.Equal($"Tomar {Composed} forte".Length, probe.Length);
    }

    [Fact]
    public async Task Whitespace_around_the_value_is_trimmed()
    {
        var probe = await PostAsync("""{"title":"   Comprar leite   "}""");

        Assert.Equal("Comprar leite", probe.Title);
    }

    /// <remarks>
    /// Nulo não pode virar texto vazio: um campo obrigatório ausente tem que continuar sendo
    /// rejeitado pela validação, com 400, e não passar adiante como string em branco.
    /// </remarks>
    [Fact]
    public async Task Null_stays_null()
    {
        var probe = await PostAsync("""{"title":"a","optional":null}""");

        Assert.Null(probe.Optional);
    }

    /// <remarks>
    /// O outro sentido da porta. O valor sai do servidor sem passar pela entrada, e tem que
    /// chegar ao cliente como está: normalizar na saída mascararia um texto que escapou da
    /// normalização, consertando no caminho de volta um defeito que precisa ser visto.
    ///
    /// O cliente lê com as opções padrão, sem o conversor, então o que ele mede é o que o
    /// servidor escreveu.
    /// </remarks>
    [Fact]
    public async Task Output_is_written_as_it_is()
    {
        var probe = await PostAsync("""{"title":"a"}""");

        Assert.Equal(Decomposed, probe.ServerSide);
        Assert.Equal(5, probe.ServerSide.Length);
    }
}
