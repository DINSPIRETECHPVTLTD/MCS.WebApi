using System.ComponentModel.DataAnnotations;

namespace MCS.WebApi.DTOs
{
    public class CreateMasterLookupDto
    {
        [Required]
        [StringLength(50)]
        public string LookupKey { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LookupCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LookupValue { get; set; } = string.Empty;

        public decimal? NumericValue { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Description { get; set; }
    }
}
