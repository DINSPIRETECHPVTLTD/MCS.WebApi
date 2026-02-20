using MCS.WebApi.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.DTOs.Loan
{
    public class CreateLoanDto
    {
        [Required]
        public int MemberId { get; set; }

        [Required]
        public decimal LoanAmount { get; set; }

        [Required]
        public decimal InterestAmount { get; set; }

        [Required]
        public decimal ProcessingFee { get; set; }

        [Required]
        public decimal InsuranceFee { get; set; }

        [Required]
        public bool IsSavingEnabled { get; set; }

        [Required]
        public decimal SavingAmount { get; set; }
        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime? DisbursementDate { get; set; }

        [Required]
        public DateTime? CollectionStartDate { get; set; }

        [Required]
        public string CollectionTerm { get; set; } 

        [Required]
        public int NoOfTerms { get; set; }
    }
}
