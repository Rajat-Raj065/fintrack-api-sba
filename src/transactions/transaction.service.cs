using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FintrackApi.Transactions
{
    /// <summary>
    /// Service contract for transaction operations.
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Creates and persists a transaction.
        /// </summary>
        Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns all transactions for the specified user.
        /// </summary>
        Task<List<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all transactions (admin/maintenance operation).
        /// </summary>
        Task DeleteAllAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// EF-backed implementation of <see cref="ITransactionService"/>.
    /// </summary>
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<TransactionService> _logger;

        /// <summary>
        /// Creates a new instance of <see cref="TransactionService"/>.
        /// </summary>
        public TransactionService(ApplicationDbContext db, ILogger<TransactionService> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken = default)
        {
            if (transaction is null) throw new ArgumentNullException(nameof(transaction));
            if (string.IsNullOrWhiteSpace(transaction.UserId)) throw new ArgumentException("UserId is required.", nameof(transaction.UserId));

            // Basic validation
            if (transaction.Amount == 0m) throw new ArgumentException("Amount cannot be zero.", nameof(transaction.Amount));

            // Ensure identity and timestamps
            if (transaction.Id == Guid.Empty) transaction.Id = Guid.NewGuid();
            transaction.CreatedAt = DateTimeOffset.UtcNow;

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Created transaction {TransactionId} for user {UserId}", transaction.Id, transaction.UserId);

            return transaction;
        }

        /// <inheritdoc />
        public async Task<List<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentException("userId is required", nameof(userId));

            return await _db.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            // Use EF Core 8 ExecuteDeleteAsync for efficient server-side deletion
            var deleted = await _db.Transactions.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("Deleted {Count} transactions via DeleteAllAsync", deleted);
        }
    }
}