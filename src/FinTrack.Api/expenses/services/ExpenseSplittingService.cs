using FinTrack.Api.Expenses.Contracts;
using FinTrack.Api.Expenses.Models;
using FinTrack.Api.Expenses.Repositories;
using FinTrack.Api.Transactions.Exceptions;
using Microsoft.Extensions.Logging;

namespace FinTrack.Api.Expenses.Services;

public sealed class ExpenseSplittingService : IExpenseSplittingService
{
    private readonly ISharedExpenseRepository _repository;
    private readonly ILogger<ExpenseSplittingService> _logger;

    public ExpenseSplittingService(ISharedExpenseRepository repository, ILogger<ExpenseSplittingService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Guid> CreateSharedExpenseAsync(string currentUserId, CreateSharedExpenseRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new ForbiddenTransactionAccessException("Authenticated user context is required.");

        if (request.Participants is null || request.Participants.Count < 2)
            throw new TransactionValidationException("At least 2 participants are required.");

        var distinct = request.Participants
            .Select(p => p.UserId.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (distinct.Count != request.Participants.Count)
            throw new TransactionValidationException("Participants must be unique and non-empty.");

        var splitType = request.SplitType.Trim().ToLowerInvariant();
        if (splitType is not ("equal" or "custom"))
            throw new TransactionValidationException("SplitType must be equal or custom.");

        List<SharedExpenseParticipant> participants;
        if (splitType == "equal")
        {
            var each = decimal.Round(request.TotalAmount / request.Participants.Count, 2, MidpointRounding.ToEven);
            var totalAssigned = each * request.Participants.Count;
            var delta = request.TotalAmount - totalAssigned;

            participants = request.Participants.Select((p, i) => new SharedExpenseParticipant
            {
                Id = Guid.NewGuid(),
                ParticipantUserId = p.UserId.Trim(),
                ShareAmount = i == 0 ? each + delta : each
            }).ToList();
        }
        else
        {
            if (request.Participants.Any(p => p.ShareAmount is null))
                throw new TransactionValidationException("Custom split requires share amount for every participant.");

            var sum = request.Participants.Sum(p => p.ShareAmount!.Value);
            if (decimal.Round(sum, 2) != decimal.Round(request.TotalAmount, 2))
                throw new TransactionValidationException("Custom share amounts must sum exactly to total amount.");

            participants = request.Participants.Select(p => new SharedExpenseParticipant
            {
                Id = Guid.NewGuid(),
                ParticipantUserId = p.UserId.Trim(),
                ShareAmount = decimal.Round(p.ShareAmount!.Value, 2, MidpointRounding.ToEven)
            }).ToList();
        }

        var expense = new SharedExpense
        {
            Id = Guid.NewGuid(),
            CreatorUserId = currentUserId,
            Description = request.Description.Trim(),
            TotalAmount = decimal.Round(request.TotalAmount, 2, MidpointRounding.ToEven),
            SplitType = splitType,
            CreatedAtUtc = DateTime.UtcNow,
            Participants = participants
        };

        var saved = await _repository.CreateAsync(expense, cancellationToken);

        _logger.LogInformation(
            "Shared expense created. ExpenseId:{ExpenseId} Creator:{CreatorUserId} Total:{Total} SplitType:{SplitType}",
            saved.Id, currentUserId, saved.TotalAmount, saved.SplitType);

        return saved.Id;
    }

    public async Task<IReadOnlyList<PendingBalanceResponse>> GetPendingBalancesAsync(string currentUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new ForbiddenTransactionAccessException("Authenticated user context is required.");

        var expenses = await _repository.GetExpensesForUserAsync(currentUserId, cancellationToken);

        // + means current user owes counterparty
        var ledger = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        foreach (var expense in expenses)
        {
            if (expense.CreatorUserId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase))
            {
                // others owe current user => negative for current user perspective
                foreach (var p in expense.Participants.Where(x => !x.ParticipantUserId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase)))
                {
                    ledger[p.ParticipantUserId] = ledger.GetValueOrDefault(p.ParticipantUserId) - p.ShareAmount;
                }
            }
            else
            {
                var myShare = expense.Participants
                    .FirstOrDefault(x => x.ParticipantUserId.Equals(currentUserId, StringComparison.OrdinalIgnoreCase));

                if (myShare is not null)
                {
                    ledger[expense.CreatorUserId] = ledger.GetValueOrDefault(expense.CreatorUserId) + myShare.ShareAmount;
                }
            }
        }

        var result = ledger
            .Where(kv => kv.Value != 0)
            .Select(kv => new PendingBalanceResponse
            {
                CounterpartyUserId = kv.Key,
                NetAmount = decimal.Round(kv.Value, 2, MidpointRounding.ToEven)
            })
            .OrderBy(x => x.CounterpartyUserId)
            .ToList();

        return result;
    }
}