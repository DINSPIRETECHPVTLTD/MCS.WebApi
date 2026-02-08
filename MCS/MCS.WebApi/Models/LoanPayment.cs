using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    public class LoanPayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LoanId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SavingAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PenaltyAmount { get; set; } = 0;

        public DateTime? ActualPaymentDate { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        public int InstallmentNo { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Not Paid"; // Paid, Partial, Not Paid

        [StringLength(50)]
        public string? PaymentMode { get; set; } // Cash, Branch Bank Account, UPI, Other

        [StringLength(100)]
        public string? ReceivedBy { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Computed property for total payment amount
        [NotMapped]
        public decimal TotalPaymentAmount => PrincipalAmount + InterestAmount + PenaltyAmount + (SavingAmount ?? 0);

        // Navigation properties
        [ForeignKey("LoanId")]
        public virtual Loan Loan { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey("ModifiedBy")]
        public virtual User? ModifiedByUser { get; set; }
    }
}
