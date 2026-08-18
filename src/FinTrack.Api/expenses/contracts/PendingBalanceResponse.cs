namespace FinTrack.Api.Expenses.Contracts;

public sealed class PendingBalanceResponse
{
    public string CounterpartyUserId { get; set; } = string.Empty;
    public decimal NetAmount { get; set; } // + => current user owes counterparty, - => counterparty owes current user
}