using MCS.WebApi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.DTOs.Loan
{
    public class LoanDto
    {
        public int Id { get; set; }

        public int MemberId { get; set; }

        public decimal LoanAmount { get; set; }

        public decimal InterestAmount { get; set; }

        public decimal ProcessingFee { get; set; }

        public decimal InsuranceFee { get; set; }

        public bool IsSavingEnabled { get; set; }

        public decimal SavingAmount { get; set; }

 
        public decimal TotalAmount { get; set; }

 
        public string Status { get; set; } 

        public DateTime? DisbursementDate { get; set; }

        public DateTime? ClosureDate { get; set; }

        public DateTime? CollectionStartDate { get; set; }

         public string CollectionTerm { get; set; } = string.Empty;

        public int NoOfTerms { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        
    }

}
