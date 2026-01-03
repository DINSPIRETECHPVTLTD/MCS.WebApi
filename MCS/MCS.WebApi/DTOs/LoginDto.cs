namespace MCS.WebApi.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int? OrganizationId { get; set; }
        public int? BranchId { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}

