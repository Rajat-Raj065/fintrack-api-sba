using System.ComponentModel.DataAnnotations;

namespace FinTrack.Api.Transactions.Contracts;

/// <summary>
/// Request contract for creating a transaction.
/// </summary>
public sealed class CreateTransactionRequest
{
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999.99")]
    public decimal Amount { get; set; }

    [Required]
    [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Currency must be a 3-letter uppercase ISO code.")]
    public string Currency { get; set; } = "USD";
}