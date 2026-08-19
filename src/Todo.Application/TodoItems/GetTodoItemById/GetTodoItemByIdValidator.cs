using FluentValidation;

namespace Todo.Application.TodoItems.GetTodoItemById;

internal sealed class GetTodoItemByIdValidator : AbstractValidator<GetTodoItemByIdRequest>
{
    public GetTodoItemByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
