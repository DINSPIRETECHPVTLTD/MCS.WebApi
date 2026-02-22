using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    /// <summary>Known lookup key constants. UI derives dropdown options from GET MasterLookups (distinct LookupKey).</summary>
    public static class LookupKeys
    {
        public const string LoanTerm = "LOAN_TERM";
        public const string PaymentType = "PAYMENT_TYPE";
        public const string Relationship = "RELATIONSHIP";
        public const string State = "STATE";
        public const string PaymentMode = "PAYMENTMODE";
    }

    [Table("MasterLookups")]
    public class MasterLookup
    {
        public int Id { get; set; }

        // Category key (LOAN_TERM, PAYMENT_TYPE, etc.)
        public string LookupKey { get; set; } = null!;

        // Unique code inside the category (LT_12, MONTHLY, etc.)
        public string LookupCode { get; set; } = null!;

        // Display text
        public string LookupValue { get; set; } = null!;

        // Optional numeric value (months, interest %, etc.)
        public decimal? NumericValue { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Description { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedOn { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
