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
/// Verifica que o corpo JSON é uma porta: data entra em UTC e sai com o sufixo <c>Z</c>.
/// </summary>
/// <remarks>
/// Esta é a porta que alimenta os casos de uso de escrita, e nada no miolo normaliza data —
/// validações e domínio comparam direto. Se o <c>UtcDateTimeJsonConverter</c> deixar de ser
/// registrado, ou parar de converter, a falha aparece aqui e não como decisão de negócio errada
/// lá dentro.
///
/// O host chama <see cref="DependencyInjection.AddApi"/> de propósito: o que está sob teste é o
/// registro do converter pelo código de produção, e não o converter isolado. O endpoint de
/// sondagem devolve o <see cref="DateTimeKind"/> porque é ele que se perde no caminho.
/// </remarks>
public sealed class UtcDateTimeJsonBodyTests : IAsyncLifetime
{
    private sealed record ProbeRequest(DateTime DueDate, DateTime? Optional);

    private sealed record ProbeResponse(string Kind, string Roundtrip, DateTime Rendered, string? OptionalKind);

    private WebApplication app = null!;

    private HttpClient client = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.WebHost.UseTestServer();
        builder.Services.AddApi(new ConfigurationBuilder().Build());

        app = builder.Build();

        new ApiEndpointGroup().MapGroup(app).MapPost("/probe", (ProbeRequest body) =>
            new ProbeResponse(
                body.DueDate.Kind.ToString(),
                body.DueDate.ToString("O"),
                body.DueDate,
                body.Optional?.Kind.ToString()));

        await app.StartAsync(TestContext.Current.CancellationToken);

        client = app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        client.Dispose();

        await app.DisposeAsync();
    }

    /// <remarks>
    /// Envia o JSON como texto cru, e lê a resposta como texto: se o teste serializasse e
    /// desserializasse com as próprias opções, mediria o cliente e não o servidor.
    /// </remarks>
    private async Task<string> PostAsync(string json)
    {
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/probe", content, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
    }

    /// <remarks>
    /// As quatro grafias descrevem o mesmo instante. Sem offset é assumido UTC, que é a decisão
    /// do projeto — a alternativa, hora local do servidor, faria a mesma requisição gravar
    /// valores diferentes conforme a máquina.
    /// </remarks>
    [Theory]
    [InlineData("2027-03-10T12:00:00Z")]
    [InlineData("2027-03-10T09:00:00-03:00")]
    [InlineData("2027-03-10T21:00:00+09:00")]
    [InlineData("2027-03-10T12:00:00")]
    public async Task Every_spelling_in_the_body_arrives_as_one_utc_instant(string written)
    {
        var body = await PostAsync($$"""{"dueDate":"{{written}}"}""");

        Assert.Contains("\"kind\":\"Utc\"", body);
        Assert.Contains("\"roundtrip\":\"2027-03-10T12:00:00.0000000Z\"", body);
    }

    /// <remarks>
    /// O outro sentido da porta. Sem o sufixo <c>Z</c> a resposta é ambígua, e o cliente lê o
    /// instante como hora local dele.
    /// </remarks>
    [Fact]
    public async Task Date_in_the_response_carries_the_utc_suffix()
    {
        var body = await PostAsync("""{"dueDate":"2027-03-10T09:00:00-03:00"}""");

        Assert.Contains("\"rendered\":\"2027-03-10T12:00:00.0000000Z\"", body);
    }

    [Fact]
    public async Task Optional_date_is_normalised_when_present_and_stays_null_when_absent()
    {
        var filled = await PostAsync("""{"dueDate":"2027-03-10T12:00:00Z","optional":"2027-03-10T09:00:00-03:00"}""");

        Assert.Contains("\"optionalKind\":\"Utc\"", filled);

        var absent = await PostAsync("""{"dueDate":"2027-03-10T12:00:00Z"}""");

        Assert.Contains("\"optionalKind\":null", absent);
    }
}
