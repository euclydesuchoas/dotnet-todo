namespace Todo.Application.Abstractions.Persistence;

/// <summary>
/// Confirma, em uma única transação, as alterações acumuladas pelos repositórios.
/// </summary>
/// <remarks>
/// Separado de <see cref="ITodoRepository"/> de propósito: o caso de uso decide quando
/// gravar, e por isso pode compor mais de um repositório antes de confirmar.
/// </remarks>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
