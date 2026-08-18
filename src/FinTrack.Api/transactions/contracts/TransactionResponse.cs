namespace FinTrack.Api.Transactions.Contracts;

/// <summary>
/// API response contract for transaction records.
/// </summary>
public sealed class TransactionResponse
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime CreatedAtUtc { get; set; }
}