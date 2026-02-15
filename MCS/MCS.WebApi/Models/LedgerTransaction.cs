using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    [Table("LedgerTransactions")]
    public class LedgerTransaction
    {
        [Key]
        public int Id { get; set; }

        public int? PaidFromUserId { get; set; }

        public int? PaidToUserId { get; set; }

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
        public int? ReferenceId { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }

        // Navigation properties
        [ForeignKey("FromUserId")]
        public virtual User? FromUser { get; set; } = null!;

        [ForeignKey("ToUserId")]
        public virtual User? ToUser { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }
    }
}
