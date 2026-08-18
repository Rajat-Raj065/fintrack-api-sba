using FinTrack.Api.Transactions.Models;

namespace FinTrack.Api.Transactions.Repositories;

/// <summary>
/// Data access abstraction for transaction entities.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Persists a new transaction.
    /// </summary>
    Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken);

    /// <summary>
    /// Returns transactions owned by a specific user.
    /// </summary>
    Task<IReadOnlyList<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all transactions for a specific user.
    /// </summary>
    Task<int> DeleteAllForUserAsync(string userId, CancellationToken cancellationToken);
}