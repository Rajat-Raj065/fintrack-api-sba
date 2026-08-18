using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinTrack.Api.Expenses.Models;

/// <summary>
/// Shared expense created by a user and split among participants.
/// </summary>
public sealed class SharedExpense
{
    public Guid Id { get; set; }

    [Required, MaxLength(128)]
    public string CreatorUserId { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Required, MaxLength(10)]
    public string SplitType { get; set; } = "equal"; // equal/custom

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<SharedExpenseParticipant> Participants { get; set; } = new List<SharedExpenseParticipant>();
}