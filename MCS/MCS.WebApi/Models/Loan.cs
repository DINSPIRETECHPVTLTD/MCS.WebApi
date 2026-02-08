using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    [Table("Loans")]
    public class Loan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string LoanCode { get; set; } = string.Empty;

        [Required]
        public int MemberId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal LoanAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InterestAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ProcessingFee { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal InsuranceFee { get; set; }

        [Required]
        public bool IsSavingEnabled { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SavingAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OutstandingAmount { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Closed, Defaulted

        public DateTime? DisbursementDate { get; set; }

        public DateTime? ClosureDate { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey("ModifiedBy")]
        public virtual User? ModifiedByUser { get; set; }

        public virtual ICollection<LoanPayment>? LoanPayments { get; set; }
    }
}
