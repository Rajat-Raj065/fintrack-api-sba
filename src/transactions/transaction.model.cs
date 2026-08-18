using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FintrackApi.Transactions
{
    /// <summary>
    /// Represents a user transaction stored in the database.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Owner user id (from Identity/JWT).
        /// </summary>
        [Required]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// Monetary amount. Use decimal for currency values.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Optional description or memo.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// UTC creation timestamp.
        /// </summary>
        [Required]
        public DateTimeOffset CreatedAt { get; set; }
    }
}