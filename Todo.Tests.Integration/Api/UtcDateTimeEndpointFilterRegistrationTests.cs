using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Todo.Api.Endpoints;

namespace Todo.Tests.Integration.Api;

/// <summary>
/// Verifica que o grupo raiz da API entrega datas de query string já em UTC ao handler.
/// </summary>
/// <remarks>
/// O alvo aqui é o registro, e não o filtro: <c>UtcDateTimeEndpointFilterTests</c> cobre o que
/// o filtro faz com os argumentos, mas passaria intacto se alguém removesse o
/// <c>AddEndpointFilter</c> de <see cref="ApiEndpointGroup"/>. Este teste monta o grupo pelo
/// código de produção e mede o que chega do outro lado do vínculo.
///
/// Sobe um host com <c>TestServer</c> em vez de usar o <c>Program</c> inteiro: o que está sob
/// teste é o grupo, e um endpoint de sondagem mapeado nele exercita o mesmo caminho sem exigir
/// banco, migration nem configuração da aplicação.
///
/// A sondagem devolve o <see cref="DateTimeKind"/> porque é ele que se perde — e ele não é
/// observável pelos endpoints reais contra SQLite: sem o filtro, uma data sem offset chega com
/// os dígitos certos e <see cref="DateTimeKind.Unspecified"/>, e tanto a comparação de
/// <see cref="DateTime"/> quanto o conversor do EF Core a tratam como UTC assim mesmo. Quem
/// recusa o valor é o Npgsql, no Postgres, que não está disponível na suíte.
/// </remarks>
public sealed class UtcDateTimeEndpointFilterRegistrationTests : IAsyncLifetime
{
    private sealed record Probe(string Kind, DateTime Value, string? NullableKind, DateTime? NullableValue);

    private WebApplication app = null!;

    private HttpClient client = null!;

    public async ValueTask InitializeAsync()
    {
        var builder = WebApplication.CreateSlimBuilder();

        builder.WebHost.UseTestServer();

        app = builder.Build();

        // O grupo vem do código de produção: é o registro do filtro que está sob teste.
        var group = new ApiEndpointGroup().MapGroup(app);

        group.MapGet("/probe", (DateTime dueDate, DateTime? optional) =>
            new Probe(dueDate.Kind.ToString(), dueDate, optional?.Kind.ToString(), optional));

        await app.StartAsync(TestContext.Current.CancellationToken);

        client = app.GetTestClient();
    }

    public async ValueTask DisposeAsync()
    {
        client.Dispose();

        await app.DisposeAsync();
    }

    private async Task<Probe> GetProbeAsync(string dueDate)
    {
        var response = await client.GetFromJsonAsync<Probe>(
            $"/api/probe?dueDate={Uri.EscapeDataString(dueDate)}",
            TestContext.Current.CancellationToken);

        return response!;
    }

    /// <remarks>
    /// O caso que o filtro existe para resolver: sem offset o vínculo entrega
    /// <see cref="DateTimeKind.Unspecified"/>, e é a marcação que falta — os dígitos já estão
    /// certos.
    /// </remarks>
    [Fact]
    public async Task Date_without_offset_reaches_the_handler_as_utc()
    {
        var probe = await GetProbeAsync("2027-03-10T12:00:00");

        Assert.Equal(nameof(DateTimeKind.Utc), probe.Kind);
        Assert.Equal(new DateTime(2027, 3, 10, 12, 0, 0, DateTimeKind.Utc), probe.Value);
    }

    /// <remarks>
    /// As três grafias do mesmo instante têm que chegar como o mesmo valor em UTC. As duas com
    /// offset já chegariam assim do vínculo, que usa <c>DateTimeStyles.AdjustToUniversal</c>;
    /// estão aqui para que a equivalência seja verificada, e não presumida.
    /// </remarks>
    [Theory]
    [InlineData("2027-03-10T12:00:00Z")]
    [InlineData("2027-03-10T09:00:00-03:00")]
    [InlineData("2027-03-10T21:00:00+09:00")]
    [InlineData("2027-03-10T12:00:00")]
    public async Task Every_spelling_of_one_instant_reaches_the_handler_the_same_way(string dueDate)
    {
        var probe = await GetProbeAsync(dueDate);

        Assert.Equal(nameof(DateTimeKind.Utc), probe.Kind);
        Assert.Equal(new DateTime(2027, 3, 10, 12, 0, 0, DateTimeKind.Utc), probe.Value);
    }

    [Fact]
    public async Task Nullable_date_is_normalised_when_present_and_stays_null_when_absent()
    {
        var filled = await client.GetFromJsonAsync<Probe>(
            "/api/probe?dueDate=2027-03-10T12:00:00&optional=2027-03-10T09:00:00-03:00",
            TestContext.Current.CancellationToken);

        Assert.Equal(nameof(DateTimeKind.Utc), filled!.NullableKind);
        Assert.Equal(new DateTime(2027, 3, 10, 12, 0, 0, DateTimeKind.Utc), filled.NullableValue);

        var absent = await GetProbeAsync("2027-03-10T12:00:00");

        Assert.Null(absent.NullableKind);
        Assert.Null(absent.NullableValue);
    }
}
