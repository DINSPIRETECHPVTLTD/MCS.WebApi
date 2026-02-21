namespace MCS.WebApi.DTOs.Loan
{
    public class ActiveLoanSummaryDto
    {
        public int LoanId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public int NoOfTerms { get; set; }
        public int NumberOfPaidEmis { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public decimal TotalUnpaidAmount { get; set; }        
        public decimal TotalAmount { get; set; }
        
    }
}
