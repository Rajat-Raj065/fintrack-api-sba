using FinTrack.Api.Expenses.Contracts;

namespace FinTrack.Api.Expenses.Services;

public interface IExpenseSplittingService
{
    Task<Guid> CreateSharedExpenseAsync(string currentUserId, CreateSharedExpenseRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<PendingBalanceResponse>> GetPendingBalancesAsync(string currentUserId, CancellationToken cancellationToken);
}