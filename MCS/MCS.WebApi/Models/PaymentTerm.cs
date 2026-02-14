using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    /// <summary>
    /// Maps to PaymentTerms table (default schema, same as Branch, Member, etc.).
    /// Main branch migrations: PaymentTerm.cs creates table, PaymentTerm-AddedPaymentTypeColumn adds PaymentType.
    /// </summary>
    [Table("PaymentTerms")]
    public class PaymentTerm
    {
        [Column("PaymentTermID")]
        public int PaymentTermId { get; set; }

        [Column("PaymentTerm")]
        public string PaymentTermName { get; set; } = null!;

        [Column("PaymentType")]
        public string PaymentType { get; set; } = string.Empty;

        [Column("NoOfTerms")]
        public int NoOfTerms { get; set; }

        [Column("ProcessingFee")]
        public decimal? ProcessingFee { get; set; }

        [Column("RateOfInterest")]
        public decimal? RateOfInterest { get; set; }

        [Column("InsuranceFee")]
        public decimal? InsuranceFee { get; set; }

        [Column("CreatedBy")]
        public int CreatedBy { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }

        [Column("ModifiedBy")]
        public int? ModifiedBy { get; set; }

        [Column("ModifiedAt")]
        public DateTime? ModifiedAt { get; set; }

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; }
    }
}
