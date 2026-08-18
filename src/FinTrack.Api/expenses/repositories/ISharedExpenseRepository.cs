using FinTrack.Api.Expenses.Models;

namespace FinTrack.Api.Expenses.Repositories;

public interface ISharedExpenseRepository
{
    Task<SharedExpense> CreateAsync(SharedExpense expense, CancellationToken cancellationToken);
    Task<IReadOnlyList<SharedExpense>> GetExpensesForUserAsync(string userId, CancellationToken cancellationToken);
}