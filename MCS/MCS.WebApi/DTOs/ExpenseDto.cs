using System.ComponentModel.DataAnnotations;

namespace MCS.WebApi.DTOs
{
    public class ExpenseDto
    {
        [Required]
        public required int UserId { get; set; }
        [Required]
        public required int Amount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        public required string Comment { get; set; }
    }
}
