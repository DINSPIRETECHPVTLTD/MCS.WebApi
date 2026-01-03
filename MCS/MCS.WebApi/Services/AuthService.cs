using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MCS.WebApi.Data;
using MCS.WebApi.Models;
using MCS.WebApi.DTOs;
using BCrypt.Net;

namespace MCS.WebApi.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        string GenerateJwtToken(OrganizationUser user, string userType);
        string GenerateJwtToken(BranchUser user, string userType);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Try Organization User first
            var orgUser = _context.OrganizationUsers
                .FirstOrDefault(u => u.Email == loginDto.Email && !u.IsDeleted);

            if (orgUser != null)
            {
                if (BCrypt.Net.BCrypt.Verify(loginDto.Password, orgUser.PasswordHash))
                {
                    var token = GenerateJwtToken(orgUser, "Organization");
                    return Task.FromResult<AuthResponseDto?>(new AuthResponseDto
                    {
                        Token = token,
                        UserType = "Organization",
                        UserId = orgUser.OrgUserId,
                        OrganizationId = orgUser.OrganizationId,
                        Role = orgUser.Role.ToString()
                    });
                }
            }

            // Try Branch User
            var branchUser = _context.BranchUsers
                .FirstOrDefault(u => u.Email == loginDto.Email && !u.IsDeleted);

            if (branchUser != null)
            {
                if (BCrypt.Net.BCrypt.Verify(loginDto.Password, branchUser.PasswordHash))
                {
                    var token = GenerateJwtToken(branchUser, "Branch");
                    return Task.FromResult<AuthResponseDto?>(new AuthResponseDto
                    {
                        Token = token,
                        UserType = "Branch",
                        UserId = branchUser.BranchUserId,
                        BranchId = branchUser.BranchId,
                        Role = branchUser.Role.ToString()
                    });
                }
            }

            return Task.FromResult<AuthResponseDto?>(null);
        }

        public string GenerateJwtToken(OrganizationUser user, string userType)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.OrgUserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("UserType", userType),
                new Claim("OrganizationId", user.OrganizationId.ToString())
            };

            return GenerateToken(claims);
        }

        public string GenerateJwtToken(BranchUser user, string userType)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.BranchUserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("UserType", userType),
                new Claim("BranchId", user.BranchId.ToString())
            };

            return GenerateToken(claims);
        }

        private string GenerateToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

