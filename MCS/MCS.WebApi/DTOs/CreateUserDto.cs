using MCS.WebApi.Models;
using System.ComponentModel.DataAnnotations;

namespace MCS.WebApi.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string FirstName { get; set; }
        public string? MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? ZipCode { get; set; }
        public UserRole Role { get; set; }
        public UserLevel Level { get; set; }
        public int? BranchId { get; set; }
    }
}

