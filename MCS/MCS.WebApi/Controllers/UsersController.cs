using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MCS.WebApi.Data;
using MCS.WebApi.Models;
using MCS.WebApi.DTOs;
using BCrypt.Net;

namespace MCS.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null || user.Role != UserRole.Owner)
            {
                return Forbid();
            }

            // Return only User properties without navigation properties to avoid circular references
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.OrgId == user.OrgId)
                .Select(u => new User
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    MiddleName = u.MiddleName,
                    LastName = u.LastName,
                    Role = u.Role,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address1 = u.Address1,
                    Address2 = u.Address2,
                    City = u.City,
                    State = u.State,
                    ZipCode = u.ZipCode,
                    OrgId = u.OrgId,
                    Level = u.Level,
                    BranchId = u.BranchId,
                    CreatedBy = u.CreatedBy,
                    CreatedAt = u.CreatedAt,
                    ModifiedBy = u.ModifiedBy,
                    ModifiedAt = u.ModifiedAt,
                    IsDeleted = u.IsDeleted
                })
                .ToListAsync();
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var currentUser = await _context.Users.FindAsync(userId);
            
            if (currentUser == null)
            {
                return Forbid();
            }

            // Return only User properties without navigation properties to avoid circular references
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id && u.OrgId == currentUser.OrgId)
                .Select(u => new User
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    MiddleName = u.MiddleName,
                    LastName = u.LastName,
                    Role = u.Role,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address1 = u.Address1,
                    Address2 = u.Address2,
                    City = u.City,
                    State = u.State,
                    ZipCode = u.ZipCode,
                    OrgId = u.OrgId,
                    Level = u.Level,
                    BranchId = u.BranchId,
                    CreatedBy = u.CreatedBy,
                    CreatedAt = u.CreatedAt,
                    ModifiedBy = u.ModifiedBy,
                    ModifiedAt = u.ModifiedAt,
                    IsDeleted = u.IsDeleted
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/Users
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<User>> PostUser(CreateUserDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var currentUser = await _context.Users.FindAsync(userId);
            
            if (currentUser == null || currentUser.Role != UserRole.Owner)
            {
                return Forbid();
            }

            // Validate BranchId if Level is Branch
            if (dto.Level == UserLevel.Branch)
            {
                if (!dto.BranchId.HasValue)
                {
                    return BadRequest("BranchId is required when Level is Branch");
                }

                var branch = await _context.Branches.FindAsync(dto.BranchId.Value);
                if (branch == null || branch.OrgId != currentUser.OrgId)
                {
                    return BadRequest("Invalid branch");
                }
            }
            else
            {
                dto.BranchId = null;
            }

            var user = new User
            {
                OrgId = currentUser.OrgId,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Address1 = dto.Address1,
                Address2 = dto.Address2,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode,
                Role = dto.Role,
                Level = dto.Level,
                BranchId = dto.BranchId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Reload user without navigation properties to avoid circular references
            var createdUser = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == user.Id)
                .Select(u => new User
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    MiddleName = u.MiddleName,
                    LastName = u.LastName,
                    Role = u.Role,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Address1 = u.Address1,
                    Address2 = u.Address2,
                    City = u.City,
                    State = u.State,
                    ZipCode = u.ZipCode,
                    OrgId = u.OrgId,
                    Level = u.Level,
                    BranchId = u.BranchId,
                    CreatedBy = u.CreatedBy,
                    CreatedAt = u.CreatedAt,
                    ModifiedBy = u.ModifiedBy,
                    ModifiedAt = u.ModifiedAt,
                    IsDeleted = u.IsDeleted
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, createdUser);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> PutUser(int id, UpdateUserDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser == null || currentUser.Role != UserRole.Owner)
            {
                return Forbid();
            }

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null || existingUser.OrgId != currentUser.OrgId)
            {
                return NotFound();
            }

            // Update only the allowed fields from DTO
            existingUser.FirstName = dto.FirstName;
            existingUser.MiddleName = dto.MiddleName;
            existingUser.LastName = dto.LastName;
            existingUser.Email = dto.Email;
            existingUser.PhoneNumber = dto.PhoneNumber;
            existingUser.Address1 = dto.Address1;
            existingUser.Address2 = dto.Address2;
            existingUser.City = dto.City;
            existingUser.State = dto.State;
            existingUser.ZipCode = dto.ZipCode;

            // Get ModifiedBy and ModifiedAt from logged-in user
            existingUser.ModifiedBy = userId;
            existingUser.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var currentUser = await _context.Users.FindAsync(userId);
            
            if (currentUser == null || currentUser.Role != UserRole.Owner)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.OrgId != currentUser.OrgId)
            {
                return NotFound();
            }

            user.IsDeleted = true;
            user.ModifiedBy = userId;
            user.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

