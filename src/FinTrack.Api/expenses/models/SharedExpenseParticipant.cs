using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrack.Api.Expenses.Models;

/// <summary>
/// Participant share in a shared expense.
/// </summary>
public sealed class SharedExpenseParticipant
{
    public Guid Id { get; set; }

    public Guid SharedExpenseId { get; set; }

    public SharedExpense? SharedExpense { get; set; }

    public string ParticipantUserId { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ShareAmount { get; set; }
}