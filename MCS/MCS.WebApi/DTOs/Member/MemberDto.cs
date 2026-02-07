namespace MCS.WebApi.DTOs.Member
{
    public class MemberDto
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? AltPhone { get; set; }


        public string? Address1 { get; set; }

        public string? Address2 { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? ZipCode { get; set; }

        public string? Aadhaar { get; set; }

        public string? Occupation { get; set; }

        public DateOnly? DOB { get; set; }

        public int Age { get; set; }


        public string GuardianFirstName { get; set; } = string.Empty;

        public string? GuardianMiddleName { get; set; }

        public string GuardianLastName { get; set; } = string.Empty;

        public string GuardianPhone { get; set; } = string.Empty;

        public DateOnly? GuardianDOB { get; set; }


        public int GuardianAge { get; set; }

        public int CenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;

        public int POCId { get; set; }
        public string POCName { get; set; } = string.Empty;


        public int CreatedBy { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public bool IsDeleted { get; set; } = false;  
    }
}
