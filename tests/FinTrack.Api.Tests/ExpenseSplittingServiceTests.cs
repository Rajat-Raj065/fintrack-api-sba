using FinTrack.Api.Expenses.Contracts;
using FinTrack.Api.Expenses.Models;
using FinTrack.Api.Expenses.Repositories;
using FinTrack.Api.Expenses.Services;
using FinTrack.Api.Transactions.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FinTrack.Api.Tests;

public sealed class ExpenseSplittingServiceTests
{
    private static ExpenseSplittingService BuildService(List<SharedExpense> store)
    {
        var repo = new InMemorySharedExpenseRepository(store);
        return new ExpenseSplittingService(repo, NullLogger<ExpenseSplittingService>.Instance);
    }

    [Fact]
    public async Task EqualSplit_Among3Participants_Works()
    {
        var store = new List<SharedExpense>();
        var service = BuildService(store);

        var request = new CreateSharedExpenseRequest
        {
            Description = "Dinner",
            TotalAmount = 90m,
            SplitType = "equal",
            Participants =
            [
                new() { UserId = "u1" },
                new() { UserId = "u2" },
                new() { UserId = "u3" }
            ]
        };

        await service.CreateSharedExpenseAsync("u1", request, CancellationToken.None);

        Assert.Single(store);
        Assert.Equal(3, store[0].Participants.Count);
        Assert.Equal(90m, store[0].Participants.Sum(p => p.ShareAmount));
    }

    [Fact]
    public async Task CustomSplit_WithMatchingTotal_Works()
    {
        var store = new List<SharedExpense>();
        var service = BuildService(store);

        var request = new CreateSharedExpenseRequest
        {
            Description = "Trip",
            TotalAmount = 100m,
            SplitType = "custom",
            Participants =
            [
                new() { UserId = "u1", ShareAmount = 40m },
                new() { UserId = "u2", ShareAmount = 30m },
                new() { UserId = "u3", ShareAmount = 30m }
            ]
        };

        await service.CreateSharedExpenseAsync("u1", request, CancellationToken.None);

        Assert.Single(store);
        Assert.Equal(100m, store[0].Participants.Sum(p => p.ShareAmount));
    }

    [Fact]
    public async Task CustomSplit_WithInvalidSum_FailsValidation()
    {
        var service = BuildService([]);

        var request = new CreateSharedExpenseRequest
        {
            Description = "Invalid",
            TotalAmount = 100m,
            SplitType = "custom",
            Participants =
            [
                new() { UserId = "u1", ShareAmount = 50m },
                new() { UserId = "u2", ShareAmount = 20m }
            ]
        };

        await Assert.ThrowsAsync<TransactionValidationException>(() =>
            service.CreateSharedExpenseAsync("u1", request, CancellationToken.None));
    }

    [Fact]
    public async Task NetBalance_BetweenTwoUsers_AggregatesCorrectly()
    {
        var store = new List<SharedExpense>();
        var service = BuildService(store);

        await service.CreateSharedExpenseAsync("A", new CreateSharedExpenseRequest
        {
            Description = "Expense1",
            TotalAmount = 60m,
            SplitType = "equal",
            Participants = [ new() { UserId = "A" }, new() { UserId = "B" } ] // B owes A 30
        }, CancellationToken.None);

        await service.CreateSharedExpenseAsync("B", new CreateSharedExpenseRequest
        {
            Description = "Expense2",
            TotalAmount = 20m,
            SplitType = "equal",
            Participants = [ new() { UserId = "A" }, new() { UserId = "B" } ] // A owes B 10
        }, CancellationToken.None);

        var balancesForA = await service.GetPendingBalancesAsync("A", CancellationToken.None);
        var withB = balancesForA.Single(x => x.CounterpartyUserId == "B");

        Assert.Equal(-20m, withB.NetAmount); // net: B owes A 20
    }

    [Fact]
    public async Task SingleParticipant_FailsValidation()
    {
        var service = BuildService([]);

        var request = new CreateSharedExpenseRequest
        {
            Description = "Solo",
            TotalAmount = 25m,
            SplitType = "equal",
            Participants = [ new() { UserId = "u1" } ]
        };

        await Assert.ThrowsAsync<TransactionValidationException>(() =>
            service.CreateSharedExpenseAsync("u1", request, CancellationToken.None));
    }

    [Fact]
    public async Task UnauthorizedAccess_ThrowsForbidden()
    {
        var service = BuildService([]);

        await Assert.ThrowsAsync<ForbiddenTransactionAccessException>(() =>
            service.GetPendingBalancesAsync("", CancellationToken.None));
    }

    private sealed class InMemorySharedExpenseRepository : ISharedExpenseRepository
    {
        private readonly List<SharedExpense> _store;

        public InMemorySharedExpenseRepository(List<SharedExpense> store)
        {
            _store = store;
        }

        public Task<SharedExpense> CreateAsync(SharedExpense expense, CancellationToken cancellationToken)
        {
            _store.Add(expense);
            return Task.FromResult(expense);
        }

        public Task<IReadOnlyList<SharedExpense>> GetExpensesForUserAsync(string userId, CancellationToken cancellationToken)
        {
            var result = _store
                .Where(e => e.CreatorUserId == userId || e.Participants.Any(p => p.ParticipantUserId == userId))
                .ToList()
                .AsReadOnly();

            return Task.FromResult((IReadOnlyList<SharedExpense>)result);
        }
    }
}