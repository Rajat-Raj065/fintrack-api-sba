using FinTrack.Api.Transactions.Contracts;

namespace FinTrack.Api.Transactions.Services;

/// <summary>
/// Transaction business operations.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Creates a user-owned transaction from request data.
    /// </summary>
    Task<TransactionResponse> CreateAsync(string currentUserId, CreateTransactionRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all transactions for the authenticated user.
    /// </summary>
    Task<IReadOnlyList<TransactionResponse>> GetByCurrentUserAsync(string currentUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all transactions for the authenticated user.
    /// </summary>
    Task<int> DeleteAllForCurrentUserAsync(string currentUserId, CancellationToken cancellationToken);
}