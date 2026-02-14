namespace MCS.WebApi.DTOs
{
    /// <summary>
    /// Request DTO for create and update. Frontend sends camelCase (paymentTerm, noOfTerms, processingFee, rateOfInterest, insuranceFee).
    /// </summary>
    public class CreatePaymentTermDto
    {
        public string PaymentTerm { get; set; } = string.Empty; // "Daily", "Weekly", "Monthly"
        public int NoOfTerms { get; set; }
        public decimal? ProcessingFee { get; set; }
        public decimal? RateOfInterest { get; set; }
        public decimal? InsuranceFee { get; set; }
    }

    /// <summary>
    /// Response DTO for GET. Property names match frontend (camelCase in JSON).
    /// </summary>
    public class PaymentTermResponseDto
    {
        public int PaymentTermId { get; set; }
        public string PaymentTerm { get; set; } = string.Empty;
        public int NoOfTerms { get; set; }
        public decimal? ProcessingFee { get; set; }
        public decimal? RateOfInterest { get; set; }
        public decimal? InsuranceFee { get; set; }
        public bool IsDeleted { get; set; }
    }
}
