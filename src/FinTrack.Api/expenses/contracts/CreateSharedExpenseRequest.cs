using System.ComponentModel.DataAnnotations;

namespace FinTrack.Api.Expenses.Contracts;

public sealed class CreateSharedExpenseRequest
{
    [Required, MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999.99")]
    public decimal TotalAmount { get; set; }

    [Required]
    [RegularExpression("^(equal|custom)$", ErrorMessage = "SplitType must be 'equal' or 'custom'.")]
    public string SplitType { get; set; } = "equal";

    [MinLength(2)]
    public List<ParticipantShareRequest> Participants { get; set; } = new();
}

public sealed class ParticipantShareRequest
{
    [Required, MaxLength(128)]
    public string UserId { get; set; } = string.Empty;

    public decimal? ShareAmount { get; set; } // required for custom
}