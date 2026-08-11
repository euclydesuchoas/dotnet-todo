using FluentValidation;
using Todo.Domain.Common;
using Todo.Domain.Todos;

namespace Todo.Application.Todos.CreateTodo;

internal sealed class CreateTodoValidator : AbstractValidator<CreateTodoRequest>
{
    public CreateTodoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            // Os limites vêm do domínio, que é também de onde a migration os tira: assim a
            // validação rejeita o que a coluna não comportaria, em vez de o banco truncar.
            .MaximumLength(TodoItem.TitleMaxLength).WithMessage($"Title must not exceed {TodoItem.TitleMaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(TodoItem.DescriptionMaxLength).WithMessage($"Description must not exceed {TodoItem.DescriptionMaxLength} characters.");

        // Must, e não GreaterThanOrEqualTo(DateTime.UtcNow): o valor constante seria avaliado
        // ao construir o validator, e não a cada validação. Hoje o registro é scoped, então a
        // diferença é de microssegundos — mas o limite congelaria no boot se o tempo de vida
        // mudasse para singleton.
        //
        // Normaliza antes de comparar porque comparação entre DateTime ignora o
        // DateTimeKind: sem isso, um valor Local seria confrontado com UtcNow pelo relógio de
        // parede, rejeitando datas futuras dentro de uma janela do tamanho do offset. Pela API
        // o UtcDateTimeJsonConverter já entregaria em UTC, mas a regra não pode depender de
        // quem chamou — o validador é público e nem todo caminho passa por HTTP.
        RuleFor(x => x.DueDate)
            .Must(dueDate => UtcDateTime.Normalize(dueDate) >= DateTime.UtcNow)
            .WithMessage("Due date must be in the future.");
    }
}
