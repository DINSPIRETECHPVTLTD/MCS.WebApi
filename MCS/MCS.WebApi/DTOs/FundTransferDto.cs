using System.ComponentModel.DataAnnotations;

namespace MCS.WebApi.DTOs
{
    public class FundTransferDto
    {
        [Required]
        public required int PaidFromUserId { get; set; }

        [Required]
        public required int PaidToUserId { get; set; }

        [Required]
        public required int Amount { get; set; }

        [Required]
        public DateTime TransferDate { get; set; }
    }
}
