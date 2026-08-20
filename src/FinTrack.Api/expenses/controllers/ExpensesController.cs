using System.Security.Claims;
using FinTrack.Api.Expenses.Contracts;
using FinTrack.Api.Expenses.Services;
using FinTrack.Api.Transactions.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Expenses.Controllers;

[ApiController]
[Route("api/expenses")]
[AllowAnonymous] // Allow anonymous for testing
public sealed class ExpensesController : ControllerBase
{
    private readonly IExpenseSplittingService _service;

    public ExpensesController(IExpenseSplittingService service)
    {
        _service = service;
    }

    [HttpPost("shared")]
    public async Task<IActionResult> CreateSharedExpense(
        [FromBody] CreateSharedExpenseRequest request,
        [FromHeader(Name = "X-User-Id")] string? headerUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId(headerUserId);
            var id = await _service.CreateSharedExpenseAsync(userId, request, cancellationToken);
            return Created(string.Empty, new { expenseId = id });
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

    [HttpGet("balances/pending")]
    public async Task<IActionResult> GetPendingBalances(
        [FromHeader(Name = "X-User-Id")] string? headerUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId(headerUserId);
            var balances = await _service.GetPendingBalancesAsync(userId, cancellationToken);
            return Ok(balances);
        }
        catch (ForbiddenTransactionAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets user ID from JWT claim OR from X-User-Id header (for testing).
    /// </summary>
    private string GetCurrentUserId(string? headerUserId)
    {
        // First try JWT claim
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Fallback to X-User-Id header for testing
        if (string.IsNullOrWhiteSpace(userId))
        {
            userId = headerUserId;
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ForbiddenTransactionAccessException("User ID is required. Provide JWT token or X-User-Id header.");
        }

        return userId;
    }
}