using FluentValidation;
using Todo.Domain.TodoItems;

namespace Todo.Application.TodoItems.GetTodoItems;

internal sealed class GetTodoItemsValidator : AbstractValidator<GetTodoItemsRequest>
{
    public GetTodoItemsValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(TodoItem.TitleMaxLength)
            .WithMessage($"Title must not exceed {TodoItem.TitleMaxLength} characters.");

        // A comparação vale como comparação de instantes porque os dois lados chegam
        // normalizados da borda, independentemente do offset que o cliente escreveu — um
        // DateTime sozinho não carrega essa garantia, já que a comparação ignora o Kind.
        // Must, e não GreaterThanOrEqualTo: o FluentValidation exige IComparable no tipo da
        // propriedade, e Nullable<T> não satisfaz restrição de interface.
        RuleFor(x => x.DueTo)
            .Must((request, dueTo) => dueTo!.Value >= request.DueFrom!.Value)
            .When(x => x.DueFrom.HasValue && x.DueTo.HasValue)
            .WithMessage("Due to must be greater than or equal to due from.");
    }
}
