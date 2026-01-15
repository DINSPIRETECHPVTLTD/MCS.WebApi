using MCS.WebApi.DTOs.Branch;
using MCS.WebApi.DTOs.Organization;

namespace MCS.WebApi.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public required OrganizationWithBranchDto Organization { get; set; }
        public string Role { get; set; } = string.Empty;
    }
}
