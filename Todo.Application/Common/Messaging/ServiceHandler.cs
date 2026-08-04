using FluentValidation;
using FluentValidation.Results;
using Todo.Application.Abstractions.Messaging;
using Todo.Application.Common.Results;

namespace Todo.Application.Common.Messaging;

internal sealed class ServiceHandler<TRequest>(
    IServiceHandler<TRequest> decoratedHandler,
    IEnumerable<IValidator<TRequest>> validators
) : IServiceHandler<TRequest>
    where TRequest : IRequest
{
    public async Task<Result> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var validationFailures = await ServiceHandlerHelper.ValidateAsync(request, validators, cancellationToken);

        if (validationFailures.Length is 0)
        {
            return await decoratedHandler.HandleAsync(request, cancellationToken);
        }

        return Result.Failure(ResultError.Validation(validationFailures));
    }
}

internal sealed class ServiceHandler<TRequest, TResponse>(
    IServiceHandler<TRequest, TResponse> decoratedHandler,
    IEnumerable<IValidator<TRequest>> validators
) : IServiceHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var validationFailures = await ServiceHandlerHelper.ValidateAsync(request, validators, cancellationToken);

        if (validationFailures.Length is 0)
        {
            return await decoratedHandler.HandleAsync(request, cancellationToken);
        }

        return Result.Failure<TResponse>(ResultError.Validation(validationFailures));
    }
}

file static class ServiceHandlerHelper
{
    internal static async Task<ValidationFailure[]> ValidateAsync<TRequest>(
        TRequest request,
        IEnumerable<IValidator<TRequest>> validators,
        CancellationToken cancellationToken
    )
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (!(validators?.Any() ?? false))
        {
            return [];
        }

        var validationResults = await Task.WhenAll(
            validators.Select(x => x.ValidateAsync(request, cancellationToken))
        );

        var validationFailures = validationResults
            .Where(x => !x.IsValid)
            .SelectMany(x => x.Errors)
            .Where(x => x != null)
            .ToArray();

        return validationFailures;
    }
}
