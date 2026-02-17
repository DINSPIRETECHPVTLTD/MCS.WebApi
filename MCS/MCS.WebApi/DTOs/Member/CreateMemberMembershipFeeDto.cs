using System.ComponentModel.DataAnnotations;

namespace MCS.WebApi.DTOs.Member
{
    public class CreateMemberMembershipFeeDto
    {
        [Required]
        public decimal Amount { get; set; }

        public DateTime? PaidDate { get; set; }

        public string? PaymentMode { get; set; }

        public string? Comments { get; set; }
    }
}