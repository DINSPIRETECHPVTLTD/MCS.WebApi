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

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("LoanId")]
        public virtual Loan Loan { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }
    }
}
