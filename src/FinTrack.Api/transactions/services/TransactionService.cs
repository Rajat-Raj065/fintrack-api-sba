using FinTrack.Api.Transactions.Contracts;
using FinTrack.Api.Transactions.Exceptions;
using FinTrack.Api.Transactions.Models;
using FinTrack.Api.Transactions.Repositories;
using Microsoft.Extensions.Logging;

namespace FinTrack.Api.Transactions.Services;

/// <summary>
/// Contains transaction business logic and validation.
/// </summary>
public sealed class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _repository;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(ITransactionRepository repository, ILogger<TransactionService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<TransactionResponse> CreateAsync(string currentUserId, CreateTransactionRequest request, CancellationToken cancellationToken)
    {
        ValidateCurrentUser(currentUserId);

        if (request.Amount <= 0)
        {
            throw new TransactionValidationException("Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new TransactionValidationException("Description is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
        {
            throw new TransactionValidationException("Currency must be a 3-letter ISO code.");
        }

        var entity = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Description = request.Description.Trim(),
            Amount = decimal.Round(request.Amount, 2, MidpointRounding.ToEven),
            Currency = request.Currency.Trim().ToUpperInvariant(),
            CreatedAtUtc = DateTime.UtcNow
        };

        var saved = await _repository.CreateAsync(entity, cancellationToken);

        _logger.LogInformation(
            "Transaction created. UserId:{UserId} TransactionId:{TransactionId} Amount:{Amount} Currency:{Currency}",
            currentUserId, saved.Id, saved.Amount, saved.Currency);

        return Map(saved);
    }

    public async Task<IReadOnlyList<TransactionResponse>> GetByCurrentUserAsync(string currentUserId, CancellationToken cancellationToken)
    {
        ValidateCurrentUser(currentUserId);

        var items = await _repository.GetByUserAsync(currentUserId, cancellationToken);

        _logger.LogInformation(
            "Transactions retrieved. UserId:{UserId} Count:{Count}",
            currentUserId, items.Count);

        return items.Select(Map).ToList();
    }

    public async Task<int> DeleteAllForCurrentUserAsync(string currentUserId, CancellationToken cancellationToken)
    {
        ValidateCurrentUser(currentUserId);

        var deletedCount = await _repository.DeleteAllForUserAsync(currentUserId, cancellationToken);

        _logger.LogWarning(
            "User transactions deleted. UserId:{UserId} DeletedCount:{DeletedCount}",
            currentUserId, deletedCount);

        return deletedCount;
    }

    private static void ValidateCurrentUser(string currentUserId)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            throw new ForbiddenTransactionAccessException("Authenticated user context is required.");
        }
    }

    private static TransactionResponse Map(Transaction t) =>
        new()
        {
            Id = t.Id,
            Description = t.Description,
            Amount = t.Amount,
            Currency = t.Currency,
            CreatedAtUtc = t.CreatedAtUtc
        };
}