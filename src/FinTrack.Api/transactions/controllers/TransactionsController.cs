using System.Security.Claims;
using FinTrack.Api.Transactions.Contracts;
using FinTrack.Api.Transactions.Exceptions;
using FinTrack.Api.Transactions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Transactions.Controllers;

/// <summary>
/// Transaction endpoints for authenticated users.
/// </summary>
[ApiController]
[Route("api/transactions")]
[Authorize]
public sealed class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionsController"/> class.
    /// </summary>
    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Creates a transaction for the authenticated user.
    /// </summary>
    /// <param name="request">Transaction create request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created transaction.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTransactionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var created = await _transactionService.CreateAsync(currentUserId, request, cancellationToken);
            return Created(string.Empty, created);
        }
        catch (TransactionValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ForbiddenTransactionAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all transactions owned by the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of user transactions.</returns>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(IReadOnlyList<TransactionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var items = await _transactionService.GetByCurrentUserAsync(currentUserId, cancellationToken);
            return Ok(items);
        }
        catch (ForbiddenTransactionAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Deletes all transactions owned by the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deleted count.</returns>
    [HttpDelete("mine")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteMine(CancellationToken cancellationToken)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var deletedCount = await _transactionService.DeleteAllForCurrentUserAsync(currentUserId, cancellationToken);
            return Ok(new { deletedCount });
        }
        catch (ForbiddenTransactionAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    private string GetCurrentUserId()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ForbiddenTransactionAccessException("Authenticated user context is required.");
        }

        return userId;
    }
}