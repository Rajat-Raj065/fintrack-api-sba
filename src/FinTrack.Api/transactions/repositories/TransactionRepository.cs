using FinTrack.Api.Transactions.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Api.Transactions.Repositories;

/// <summary>
/// EF Core implementation of transaction repository.
/// </summary>
public sealed class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _dbContext;

    public TransactionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken cancellationToken)
    {
        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task<IReadOnlyList<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteAllForUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions
            .Where(t => t.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}