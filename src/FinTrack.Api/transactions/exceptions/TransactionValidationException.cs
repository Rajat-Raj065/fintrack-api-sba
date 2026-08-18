namespace FinTrack.Api.Transactions.Exceptions;

/// <summary>
/// Raised when transaction input violates business validation rules.
/// </summary>
public sealed class TransactionValidationException : Exception
{
    public TransactionValidationException(string message) : base(message) { }
}