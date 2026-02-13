using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
  
    [Table("PaymentTerms")]
    public class PaymentTerm
    {
        [Key]
        [Column("PaymentTermID")]
        public int PaymentTermId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("PaymentTerm")]
        public string PaymentTermCode { get; set; } = string.Empty;

        [Column("NoOfTerms")]
        public int NoOfTerms { get; set; }

        [Column("ProcessingFee", TypeName = "decimal(18,2)")]
        public decimal? ProcessingFee { get; set; }

        [Column("RateOfInterest", TypeName = "decimal(18,2)")]
        public decimal? RateOfInterest { get; set; }

        [Column("InsuranceFee", TypeName = "decimal(18,2)")]
        public decimal? InsuranceFee { get; set; }

        [Required]
        [Column("CreatedBy", TypeName = "int")]
        public int CreatedBy { get; set; }

        [Required]
        [Column("CreatedAt", TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("ModifiedBy", TypeName = "int")]
        public int? ModifiedBy { get; set; }

        [Column("ModifiedAt", TypeName = "datetime2")]
        public DateTime? ModifiedAt { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;
    }
}

