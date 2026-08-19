using FluentValidation;

namespace Todo.Application.Todos.GetTodoById;

internal sealed class GetTodoByIdValidator : AbstractValidator<GetTodoByIdRequest>
{
    public GetTodoByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
