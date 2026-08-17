using FluentValidation;
using Todo.Domain.Common;
using Todo.Domain.Todos;

namespace Todo.Application.Todos.CreateTodo;

/// <remarks>
/// O relógio entra por injeção, e não por <see cref="DateTime.UtcNow"/>: "está no futuro" é
/// regra de negócio, e uma regra que só o relógio de parede sabe responder não tem como ser
/// verificada no limite. Com o relógio injetado, o teste diz qual é o agora e afirma o que
/// acontece exatamente nele.
/// </remarks>
internal sealed class CreateTodoValidator : AbstractValidator<CreateTodoRequest>
{
    public CreateTodoValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            // Os limites vêm do domínio, que é também de onde a migration os tira: assim a
            // validação rejeita o que a coluna não comportaria, em vez de o banco truncar.
            .MaximumLength(TodoItem.TitleMaxLength).WithMessage($"Title must not exceed {TodoItem.TitleMaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(TodoItem.DescriptionMaxLength).WithMessage($"Description must not exceed {TodoItem.DescriptionMaxLength} characters.");

        // Must, e não GreaterThanOrEqualTo(...): o valor constante seria lido ao construir o
        // validator, e não a cada validação. Hoje o registro é scoped, então a diferença é de
        // microssegundos — mas o limite congelaria no boot se o tempo de vida mudasse para
        // singleton. Dentro do Must, o relógio é consultado em cada chamada.
        //
        // Normaliza antes de comparar porque comparação entre DateTime ignora o
        // DateTimeKind: sem isso, um valor Local seria confrontado com o agora em UTC pelo
        // relógio de parede, rejeitando datas futuras dentro de uma janela do tamanho do
        // offset. Pela API o UtcDateTimeJsonConverter e o filtro de endpoint já entregariam em
        // UTC, mas este é o primeiro ponto da aplicação a tocar o valor: nem todo caminho passa
        // por HTTP, e quem chama em processo escreve DateTime.Now sem pensar duas vezes.
        RuleFor(x => x.DueDate)
            .Must(dueDate => UtcDateTime.Normalize(dueDate) >= timeProvider.GetUtcNow().UtcDateTime)
            .WithMessage("Due date must be in the future.");
    }
}
