namespace MCS.WebApi.DTOs.Loan
{
    public class CreateLoanDto
    {
        public string LoanCode { get; set; } = string.Empty;
        public int MemberId { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal InsuranceFee { get; set; }
        public bool IsSavingEnabled { get; set; }
        public decimal SavingAmount { get; set; }
    }
}
