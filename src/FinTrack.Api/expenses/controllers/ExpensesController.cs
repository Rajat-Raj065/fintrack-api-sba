using System.Security.Claims;
using FinTrack.Api.Expenses.Contracts;
using FinTrack.Api.Expenses.Services;
using FinTrack.Api.Transactions.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.Api.Expenses.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public sealed class ExpensesController : ControllerBase
{
    private readonly IExpenseSplittingService _service;

    public ExpensesController(IExpenseSplittingService service)
    {
        _service = service;
    }

    [HttpPost("shared")]
    public async Task<IActionResult> CreateSharedExpense([FromBody] CreateSharedExpenseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
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
    public async Task<IActionResult> GetPendingBalances(CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var balances = await _service.GetPendingBalancesAsync(userId, cancellationToken);
            return Ok(balances);
        }
        catch (ForbiddenTransactionAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new ForbiddenTransactionAccessException("Authenticated user context is required.");

        return userId;
    }
}