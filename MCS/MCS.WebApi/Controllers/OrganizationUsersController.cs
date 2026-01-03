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
    public class OrganizationUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrganizationUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/OrganizationUsers
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<OrganizationUser>>> GetOrganizationUsers()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
            {
                return Forbid();
            }

            return await _context.OrganizationUsers
                .Where(u => u.OrganizationId == orgUser.OrganizationId)
                .ToListAsync();
        }

        // GET: api/OrganizationUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationUser>> GetOrganizationUser(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null)
            {
                return Forbid();
            }

            var user = await _context.OrganizationUsers.FindAsync(id);
            if (user == null || user.OrganizationId != orgUser.OrganizationId)
            {
                return NotFound();
            }

            return user;
        }

        // POST: api/OrganizationUsers
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<OrganizationUser>> PostOrganizationUser(CreateOrganizationUserDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
            {
                return Forbid();
            }

            var user = new OrganizationUser
            {
                OrganizationId = orgUser.OrganizationId,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                Phone = dto.Phone,
                Address = dto.Address,
                Role = dto.Role,
                CreatedDate = DateTime.UtcNow
            };

            _context.OrganizationUsers.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrganizationUser", new { id = user.OrgUserId }, user);
        }

        // PUT: api/OrganizationUsers/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> PutOrganizationUser(int id, OrganizationUser user)
        {
            if (id != user.OrgUserId)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
            {
                return Forbid();
            }

            var existingUser = await _context.OrganizationUsers.FindAsync(id);
            if (existingUser == null || existingUser.OrganizationId != orgUser.OrganizationId)
            {
                return NotFound();
            }

            existingUser.FirstName = user.FirstName;
            existingUser.MiddleName = user.MiddleName;
            existingUser.LastName = user.LastName;
            existingUser.Phone = user.Phone;
            existingUser.Address = user.Address;
            existingUser.Role = user.Role;
            existingUser.Email = user.Email;

            // Password update should be handled via a separate endpoint for security

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/OrganizationUsers/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteOrganizationUser(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
            {
                return Forbid();
            }

            var user = await _context.OrganizationUsers.FindAsync(id);
            if (user == null || user.OrganizationId != orgUser.OrganizationId)
            {
                return NotFound();
            }

            user.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

