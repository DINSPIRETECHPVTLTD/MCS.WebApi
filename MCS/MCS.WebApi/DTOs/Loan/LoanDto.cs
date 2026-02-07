namespace MCS.WebApi.DTOs.Loan
{
    public class LoanDto
    {
        public int Id { get; set; }
        public string LoanCode { get; set; } = string.Empty;
        public int MemberId { get; set; }
        public decimal LoanAmount { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal InsuranceFee { get; set; }
        public bool IsSavingEnabled { get; set; }
        public decimal SavingAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DisbursementDate { get; set; }
        public DateTime? ClosureDate { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Member information
        public LoanMemberDto? Member { get; set; }
        
        // Payment summary
        public int TotalPayments { get; set; }
        public decimal TotalPaidAmount { get; set; }
    }

    public class LoanMemberDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string FullName => string.IsNullOrEmpty(MiddleName) 
            ? $"{FirstName} {LastName}" 
            : $"{FirstName} {MiddleName} {LastName}";
        public string PhoneNumber { get; set; } = string.Empty;
        public int CenterId { get; set; }
        
        // Center information
        public LoanCenterDto? Center { get; set; }
    }

    public class LoanCenterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
    }
}
