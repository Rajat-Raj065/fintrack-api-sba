using FinTrack.Api.Expenses.Models;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Api.Expenses.Repositories;

public sealed class SharedExpenseRepository : ISharedExpenseRepository
{
    private readonly AppDbContext _dbContext;

    public SharedExpenseRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SharedExpense> CreateAsync(SharedExpense expense, CancellationToken cancellationToken)
    {
        _dbContext.SharedExpenses.Add(expense);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return expense;
    }

    public async Task<IReadOnlyList<SharedExpense>> GetExpensesForUserAsync(string userId, CancellationToken cancellationToken)
    {
        return await _dbContext.SharedExpenses
            .AsNoTracking()
            .Include(e => e.Participants)
            .Where(e => e.CreatorUserId == userId || e.Participants.Any(p => p.ParticipantUserId == userId))
            .OrderByDescending(e => e.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}