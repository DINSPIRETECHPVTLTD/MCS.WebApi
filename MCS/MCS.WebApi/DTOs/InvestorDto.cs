using System.ComponentModel.DataAnnotations;

namespace MCS.WebApi.DTOs
{
    public class InvestorDto
    {
        [Required]
        public required int UserId { get; set; }
        [Required]
        public required int Amount { get; set; }

        [Required]
        public DateTime InvestmentDate { get; set; }
    }
}
