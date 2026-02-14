using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    [Table("LedgerTransactions")]
    public class LedgerTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FromUserId { get; set; }

        [Required]
        public int ToUserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50)]
        public string TransactionType { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ReferenceId { get; set; }

        // Navigation properties
        [ForeignKey("FromUserId")]
        public virtual User FromUser { get; set; } = null!;

        [ForeignKey("ToUserId")]
        public virtual User ToUser { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }
    }
}
