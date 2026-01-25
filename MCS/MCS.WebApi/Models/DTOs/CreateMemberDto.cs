using System.ComponentModel.DataAnnotations;

namespace MCS.WebApi.Models.DTOs
{
    public class CreateMemberDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string PhoneNumber { get; set; } = string.Empty;

        public DateOnly? DOB { get; set; }

        [Required]
        public int Age { get; set; }

        [Required]
        public string GuardianFirstName { get; set; } = string.Empty;

        [Required]
        public string GuardianLastName { get; set; } = string.Empty;

        [Required]
        public string GuardianPhone { get; set; } = string.Empty;

        public DateOnly? GuardianDOB { get; set; }

        [Required]
        public int GuardianAge { get; set; }

        [Required]
        public int CenterId { get; set; }

        [Required]
        public int POCId { get; set; }
        public string? MiddleName { get; set; }
        public string? AltPhone { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public string? Aadhaar { get; set; }
        
        public string? Occupation {  get; set; }

        public string? GuardianMiddleName { get; set; }
        // GuardianDOB already exists in your DTO
    }

}
