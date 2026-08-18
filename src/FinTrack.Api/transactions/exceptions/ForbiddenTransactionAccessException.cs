namespace FinTrack.Api.Transactions.Exceptions;

/// <summary>
/// Raised when a user attempts to access transactions they do not own.
/// </summary>
public sealed class ForbiddenTransactionAccessException : Exception
{
    public ForbiddenTransactionAccessException(string message) : base(message) { }
}