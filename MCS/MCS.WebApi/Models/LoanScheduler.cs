using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    [Table("LoanSchedulers")]
    public class LoanScheduler
    {
        [Key]
        public int LoanSchedulerId { get; set; }

        [Required]
        public int LoanId { get; set; }

        [Required]
        public DateTime ScheduleDate { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaymentAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SavingAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrincipalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestAmount { get; set; }


        [Required]
        public int InstallmentNo { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Not Paid"; // Paid, Partial, Not Paid

        [StringLength(50)]
        public string? PaymentMode { get; set; } // Cash, Branch Bank Account, UPI, Other

        public int CollectedBy { get; set; }

        [StringLength(500)]
        public string? Comments { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("LoanId")]
        public virtual Loan Loan { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey("CollectedBy")]
        public virtual User? CollectedByUser { get; set; }
    }
}
