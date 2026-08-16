namespace EnterpriseECommerce.Application.Interfaces;

/// <summary>
/// Represents a unit of work for coordinating database operations.
///
/// Multiple repository operations can be executed inside a single
/// database transaction.
/// </summary>
public interface IUnitOfWork
{
    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();
}