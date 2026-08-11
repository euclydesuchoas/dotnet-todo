using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Todo.Application;
using Todo.Application.Todos.CreateTodo;
using Todo.Domain.Todos;

namespace Todo.Tests.Unit.Todos;

public sealed class CreateTodoValidatorTests
{
    private static IValidator<CreateTodoRequest> Validator()
    {
        return new ServiceCollection()
            .AddApplication()
            .BuildServiceProvider()
            .GetRequiredService<IValidator<CreateTodoRequest>>();
    }

    private static CreateTodoRequest Request(DateTime dueDate)
    {
        return new CreateTodoRequest("Title", "Description", dueDate, IsCompleted: false);
    }

    /// <remarks>
    /// O limite precisa ser lido a cada validação. Se voltasse a ser um valor constante na
    /// construção da regra, congelaria — inofensivo enquanto o validator for scoped, quebrado
    /// no dia em que virar singleton.
    /// </remarks>
    [Fact]
    public void Due_date_slightly_in_the_future_is_accepted()
    {
        var result = Validator().Validate(Request(DateTime.UtcNow.AddSeconds(2)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Due_date_in_the_past_is_rejected()
    {
        var result = Validator().Validate(Request(DateTime.UtcNow.AddMinutes(-1)));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTodoRequest.DueDate));
    }

    /// <remarks>
    /// Antes do <c>UtcDateTimeJsonConverter</c>, uma data com offset chegava aqui como
    /// <see cref="DateTimeKind.Local"/> e a comparação com <see cref="DateTime.UtcNow"/>
    /// ignorava o <see cref="DateTimeKind"/> — datas futuras eram rejeitadas dentro de uma
    /// janela do tamanho do offset. Este teste fixa o comportamento correto: o mesmo instante,
    /// em qualquer <see cref="DateTimeKind"/>, decide igual.
    /// </remarks>
    [Fact]
    public void Same_instant_is_judged_the_same_in_any_kind()
    {
        var validator = Validator();
        var instant = DateTime.UtcNow.AddHours(1);

        var asUtc = validator.Validate(Request(instant));
        var asLocal = validator.Validate(Request(instant.ToLocalTime()));

        Assert.True(asUtc.IsValid);
        Assert.Equal(asUtc.IsValid, asLocal.IsValid);
    }

    [Fact]
    public void Title_longer_than_the_column_is_rejected()
    {
        var request = new CreateTodoRequest(
            new string('a', TodoItem.TitleMaxLength + 1),
            "Description",
            DateTime.UtcNow.AddDays(1),
            IsCompleted: false);

        var result = Validator().Validate(request);

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTodoRequest.Title));
    }
}
