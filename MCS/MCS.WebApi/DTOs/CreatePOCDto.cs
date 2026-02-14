using System.ComponentModel.DataAnnotations;

namespace MCS.WebApi.DTOs
{
    public class CreatePOCDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(20)]
        public string? AltPhone { get; set; }

        [StringLength(200)]
        public string? Address1 { get; set; }

        [StringLength(200)]
        public string? Address2 { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? ZipCode { get; set; }

        [Required]
        public int CenterId { get; set; }

        public string? CollectionDay { get; set; }

        [Required]
        public string CollectionFrequency { get; set; } = string.Empty;

        [Required]
        public int CollectionBy { get; set; }
    }
}
