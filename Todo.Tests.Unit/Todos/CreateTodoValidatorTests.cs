using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Todo.Application;
using Todo.Application.Todos.CreateTodo;
using Todo.Domain.Todos;

namespace Todo.Tests.Unit.Todos;

public sealed class CreateTodoValidatorTests
{
    /// <summary>
    /// O agora, para todos os testes desta classe.
    /// </summary>
    /// <remarks>
    /// Instante fixo, e não <see cref="DateTime.UtcNow"/>: o limite da regra é o agora, e com
    /// o relógio real não dá para afirmar o que acontece exatamente nele — só perto dele, com
    /// uma margem escolhida no chute.
    /// </remarks>
    private static readonly DateTimeOffset Now = new(2027, 3, 10, 12, 0, 0, TimeSpan.Zero);

    private static IValidator<CreateTodoRequest> Validator()
    {
        return new ServiceCollection()
            .AddApplication()
            // Depois do AddApplication: o registro posterior vence o TryAddSingleton da camada.
            .AddSingleton<TimeProvider>(new FakeTimeProvider(Now))
            .BuildServiceProvider()
            .GetRequiredService<IValidator<CreateTodoRequest>>();
    }

    private static CreateTodoRequest Request(DateTime dueDate)
    {
        return new CreateTodoRequest("Title", "Description", dueDate, IsCompleted: false);
    }

    /// <remarks>
    /// A regra é <c>&gt;=</c>, então o próprio instante do agora passa. É o caso que o relógio
    /// real não permitia escrever.
    /// </remarks>
    [Fact]
    public void Due_date_exactly_now_is_accepted()
    {
        var result = Validator().Validate(Request(Now.UtcDateTime));

        Assert.True(result.IsValid);
    }

    /// <remarks>
    /// Um tick antes do agora é o menor passado que existe: fixa a borda pelo lado de fora sem
    /// deixar folga onde caberia um erro de comparação.
    /// </remarks>
    [Fact]
    public void Due_date_one_tick_before_now_is_rejected()
    {
        var result = Validator().Validate(Request(Now.UtcDateTime.AddTicks(-1)));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTodoRequest.DueDate));
    }

    /// <remarks>
    /// O limite precisa ser lido a cada validação. Se voltasse a ser um valor constante na
    /// construção da regra, congelaria — inofensivo enquanto o validator for scoped, quebrado
    /// no dia em que virar singleton.
    /// </remarks>
    [Fact]
    public void Due_date_in_the_future_is_accepted()
    {
        var result = Validator().Validate(Request(Now.UtcDateTime.AddMinutes(1)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Due_date_in_the_past_is_rejected()
    {
        var result = Validator().Validate(Request(Now.UtcDateTime.AddMinutes(-1)));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTodoRequest.DueDate));
    }

    /// <remarks>
    /// Comparação entre <see cref="DateTime"/> ignora o <see cref="DateTimeKind"/>, então sem a
    /// normalização da regra um valor <see cref="DateTimeKind.Local"/> seria confrontado com o
    /// agora em UTC pelo relógio de parede, e datas futuras dentro de uma janela do tamanho do
    /// offset seriam rejeitadas. Este teste fixa o comportamento correto: o mesmo instante, em
    /// qualquer <see cref="DateTimeKind"/>, decide igual.
    ///
    /// Só tem o que dizer em máquina fora de UTC, porque é o fuso local que cria a divergência.
    /// O <c>Assert.NotEqual</c> deixa isso explícito em vez de o teste passar em silêncio por
    /// não ter exercitado nada.
    /// </remarks>
    [Fact]
    public void Same_instant_is_judged_the_same_in_any_kind()
    {
        var validator = Validator();
        var instant = Now.UtcDateTime.AddHours(1);
        var asLocalTime = instant.ToLocalTime();

        var asUtc = validator.Validate(Request(instant));
        var asLocal = validator.Validate(Request(asLocalTime));

        Assert.True(asUtc.IsValid);
        Assert.Equal(asUtc.IsValid, asLocal.IsValid);

        Assert.SkipWhen(
            TimeZoneInfo.Local.GetUtcOffset(instant) == TimeSpan.Zero,
            "A máquina roda em UTC: o valor Local é idêntico ao UTC e o teste não distingue nada.");

        Assert.NotEqual(instant, asLocalTime);
    }

    [Fact]
    public void Title_longer_than_the_column_is_rejected()
    {
        var request = new CreateTodoRequest(
            new string('a', TodoItem.TitleMaxLength + 1),
            "Description",
            Now.UtcDateTime.AddDays(1),
            IsCompleted: false);

        var result = Validator().Validate(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTodoRequest.Title));
    }
}
