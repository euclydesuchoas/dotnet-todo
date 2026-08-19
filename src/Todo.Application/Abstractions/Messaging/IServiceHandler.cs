using Todo.Application.Common.Results;

namespace Todo.Application.Abstractions.Messaging;

public interface IServiceHandler<TRequest>
    where TRequest : IRequest
{
    Task<Result> HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public interface IServiceHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<Result<TResponse>> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
