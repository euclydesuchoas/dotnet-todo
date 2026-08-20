using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Todo.Api.Endpoints;

namespace Todo.Tests.Integration.Api;

/// <summary>
/// Verifica que o grupo raiz da API entrega texto de query string já normalizado ao handler.
/// </summary>
/// <remarks>
/// O alvo aqui é o registro, e não o filtro: <c>TextValueEndpointFilterTests</c> cobre o que o
/// filtro faz com os argumentos, mas passaria intacto se alguém removesse o
/// <c>AddEndpointFilter</c> de <see cref="ApiEndpointGroup"/>. Este teste monta o grupo pelo
/// código de produção e mede o que chega do outro lado do vínculo.
///
/// É por este caminho que entra a agulha da busca — o parâmetro <c>title</c> de
/// <c>GET /todo-items</c>. Sem o filtro, uma busca escrita em uma forma Unicode deixa de
/// encontrar o que foi gravado pela outra porta, e as duas grafias são idênticas na tela.
///
/// As duas formas do mesmo texto são escritas com escapes, e nunca com o caractere acentuado
/// direto: qual delas o editor produziria depende do teclado de quem escreveu o teste.
/// </remarks>
public sealed class TextValueEndpointFilterRegistrationTests : IAsyncLifetime
{
    /// <summary>café — o acento como um code point só (U+00E9).</summary>
    private const string Composed = "caf\u00e9";

    /// <summary>café — <c>e</c> seguido do acento combinante (U+0301).</summary>
    private const string Decomposed = "cafe\u0301";

    private sealed record Probe(string Title, int Length, string? Optional);

    private WebApplication app = null!;

    private HttpClient client = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.WebHost.UseTestServer();

        app = builder.Build();

        // O grupo vem do código de produção: é o registro do filtro que está sob teste.
        var group = new ApiEndpointGroup().MapGroup(app);

        group.MapGet("/probe", (string title, string? optional) => new Probe(title, title.Length, optional));

        await app.StartAsync(TestContext.Current.CancellationToken);

        client = app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        client.Dispose();

        await app.DisposeAsync();
    }

    private async Task<Probe> GetProbeAsync(string title)
    {
        var response = await client.GetFromJsonAsync<Probe>(
            $"/api/probe?title={Uri.EscapeDataString(title)}",
            TestContext.Current.CancellationToken);

        return response!;
    }

    [Theory]
    [InlineData(Composed)]
    [InlineData(Decomposed)]
    public async Task Every_form_of_the_needle_reaches_the_handler_the_same_way(string title)
    {
        var probe = await GetProbeAsync(title);

        Assert.Equal(Composed, probe.Title);
        Assert.Equal(4, probe.Length);
    }

    [Fact]
    public async Task Whitespace_around_the_needle_is_trimmed()
    {
        var probe = await GetProbeAsync($"  {Decomposed}  ");

        Assert.Equal(Composed, probe.Title);
    }

    [Fact]
    public async Task Optional_value_is_normalised_when_present_and_stays_null_when_absent()
    {
        var filled = await client.GetFromJsonAsync<Probe>(
            $"/api/probe?title=a&optional={Uri.EscapeDataString($"  {Decomposed}  ")}",
            TestContext.Current.CancellationToken);

        Assert.Equal(Composed, filled!.Optional);

        var absent = await GetProbeAsync("a");

        Assert.Null(absent.Optional);
    }
}
