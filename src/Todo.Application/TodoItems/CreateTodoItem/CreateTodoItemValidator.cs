using FluentValidation;
using Todo.Domain.TodoItems;

namespace Todo.Application.TodoItems.CreateTodoItem;

/// <remarks>
/// O relógio entra por injeção, e não por <see cref="DateTime.UtcNow"/>: "está no futuro" é
/// regra de negócio, e uma regra que só o relógio de parede sabe responder não tem como ser
/// verificada no limite. Com o relógio injetado, o teste diz qual é o agora e afirma o que
/// acontece exatamente nele.
/// </remarks>
internal sealed class CreateTodoItemValidator : AbstractValidator<CreateTodoItemRequest>
{
    public CreateTodoItemValidator(TimeProvider timeProvider)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            // Os limites vêm do domínio, que é também de onde a migration os tira: assim a
            // validação rejeita o que a coluna não comportaria, em vez de o banco truncar.
            .MaximumLength(TodoItem.TitleMaxLength).WithMessage($"Title must not exceed {TodoItem.TitleMaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(TodoItem.DescriptionMaxLength).WithMessage($"Description must not exceed {TodoItem.DescriptionMaxLength} characters.");

        // Compara direto, sem normalizar: DueDate chega em UTC porque quem a recebeu de fora
        // já a converteu. Normalizar de novo aqui seria consertar no miolo a falha de uma
        // porta, e espalharia pelo resto das regras a obrigação de lembrar disso.
        //
        // Must, e não GreaterThanOrEqualTo(...): o valor constante seria lido ao construir o
        // validator, e não a cada validação. Hoje o registro é scoped, então a diferença é de
        // microssegundos — mas o limite congelaria no boot se o tempo de vida mudasse para
        // singleton. Dentro do Must, o relógio é consultado em cada chamada.
        RuleFor(x => x.DueDate)
            .Must(dueDate => dueDate >= timeProvider.GetUtcNow().UtcDateTime)
            .WithMessage("Due date must be in the future.");
    }
}
