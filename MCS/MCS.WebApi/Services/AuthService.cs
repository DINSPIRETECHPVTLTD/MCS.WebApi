using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MCS.WebApi.Data;
using MCS.WebApi.Models;
using BCrypt.Net;
using MCS.WebApi.DTOs.Auth;
using MCS.WebApi.DTOs.Organization;
using MCS.WebApi.DTOs.Branch;

namespace MCS.WebApi.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        string GenerateJwtToken(User user);
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

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && !u.IsDeleted);

            if (user == null)
            {
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            var token = GenerateJwtToken(user);
            var userType = user.Level == UserLevel.Org ? "Organization" : "Branch";

            var organization = await _context.Organizations
                .Include(o => o.Branches)
                .FirstOrDefaultAsync(o => o.Id == user.OrgId);

            if (organization == null)
            {
                return null; // Or handle appropriately if organization is required
            }

            var orgDto = new OrganizationWithBranchDto
            {
                Id = organization.Id,
                Name = organization.Name,
                Address1 = organization.Address1,
                Address2 = organization.Address2,
                City = organization.City,
                State = organization.State,
                // Country not present in Organization model
                ZipCode = organization.ZipCode,
                PhoneNumber = organization.PhoneNumber,
                Branches = new List<BranchInfoDto>()
            };

            var branchesQuery = organization.Branches.AsQueryable().Where(b => !b.IsDeleted);

            if (user.Level != UserLevel.Org && user.BranchId.HasValue)
            {
                branchesQuery = branchesQuery.Where(b => b.Id == user.BranchId.Value);
            }

            orgDto.Branches = branchesQuery.Select(b => new BranchInfoDto
            {
                Id = b.Id,
                Name = b.Name,
                Address1 = b.Address1,
                Address2 = b.Address2,
                City = b.City,
                State = b.State,
                Country = b.Country,
                ZipCode = b.ZipCode,
                PhoneNumber = b.PhoneNumber
            }).ToList();

            return new AuthResponseDto
            {
                Token = token,
                UserType = userType,
                UserId = user.Id,
                UserName = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Organization = orgDto,
                Role = user.Role.ToString()
            };
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("UserType", user.Level == UserLevel.Org ? "Organization" : "Branch"),
                new Claim("OrganizationId", user.OrgId.ToString())
            };

            if (user.BranchId.HasValue)
            {
                claims.Add(new Claim("BranchId", user.BranchId.Value.ToString()));
            }

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

