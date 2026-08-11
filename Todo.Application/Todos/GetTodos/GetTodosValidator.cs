using FluentValidation;
using Todo.Domain.Todos;

namespace Todo.Application.Todos.GetTodos;

internal sealed class GetTodosValidator : AbstractValidator<GetTodosRequest>
{
    public GetTodosValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(TodoItem.TitleMaxLength)
            .WithMessage($"Title must not exceed {TodoItem.TitleMaxLength} characters.");

        // A comparação é entre dois UtcDateTime, então compara instantes: os dois lados já
        // foram normalizados no parse, independentemente do offset que o cliente escreveu.
        // Must, e não GreaterThanOrEqualTo: o FluentValidation exige IComparable no tipo da
        // propriedade, e Nullable<T> não satisfaz restrição de interface.
        RuleFor(x => x.DueTo)
            .Must((request, dueTo) => dueTo!.Value >= request.DueFrom!.Value)
            .When(x => x.DueFrom.HasValue && x.DueTo.HasValue)
            .WithMessage("Due to must be greater than or equal to due from.");
    }
}
