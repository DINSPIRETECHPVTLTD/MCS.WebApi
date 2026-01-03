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
    public class BranchUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BranchUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/BranchUsers
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<BranchUser>>> GetBranchUsers()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
            {
                return Forbid();
            }

            return await _context.BranchUsers
                .Include(bu => bu.Branch)
                .Where(bu => bu.Branch.OrganizationId == orgUser.OrganizationId)
                .ToListAsync();
        }

        // GET: api/BranchUsers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BranchUser>> GetBranchUser(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;

            BranchUser? branchUser = null;

            if (userType == "Organization")
            {
                var orgUser = await _context.OrganizationUsers.FindAsync(userId);
                if (orgUser == null)
                {
                    return Forbid();
                }
                branchUser = await _context.BranchUsers
                    .Include(bu => bu.Branch)
                    .FirstOrDefaultAsync(bu => bu.BranchUserId == id && bu.Branch.OrganizationId == orgUser.OrganizationId);
            }
            else if (userType == "Branch")
            {
                var bu = await _context.BranchUsers.FindAsync(userId);
                if (bu == null || bu.BranchId != id)
                {
                    return Forbid();
                }
                branchUser = await _context.BranchUsers.FindAsync(id);
            }

            if (branchUser == null)
            {
                return NotFound();
            }

            return branchUser;
        }

        // POST: api/BranchUsers
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<BranchUser>> PostBranchUser(CreateBranchUserDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
            {
                return Forbid();
            }

            var branch = await _context.Branches.FindAsync(dto.BranchId);
            if (branch == null || branch.OrganizationId != orgUser.OrganizationId)
            {
                return BadRequest("Invalid branch");
            }

            var branchUser = new BranchUser
            {
                BranchId = dto.BranchId,
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

            _context.BranchUsers.Add(branchUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBranchUser", new { id = branchUser.BranchUserId }, branchUser);
        }

        // PUT: api/BranchUsers/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> PutBranchUser(int id, BranchUser branchUser)
        {
            if (id != branchUser.BranchUserId)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
            {
                return Forbid();
            }

            var existingUser = await _context.BranchUsers
                .Include(bu => bu.Branch)
                .FirstOrDefaultAsync(bu => bu.BranchUserId == id && bu.Branch.OrganizationId == orgUser.OrganizationId);

            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.FirstName = branchUser.FirstName;
            existingUser.MiddleName = branchUser.MiddleName;
            existingUser.LastName = branchUser.LastName;
            existingUser.Phone = branchUser.Phone;
            existingUser.Address = branchUser.Address;
            existingUser.Role = branchUser.Role;
            existingUser.Email = branchUser.Email;

            // Password update should be handled via a separate endpoint for security

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/BranchUsers/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteBranchUser(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
            {
                return Forbid();
            }

            var branchUser = await _context.BranchUsers
                .Include(bu => bu.Branch)
                .FirstOrDefaultAsync(bu => bu.BranchUserId == id && bu.Branch.OrganizationId == orgUser.OrganizationId);

            if (branchUser == null)
            {
                return NotFound();
            }

            branchUser.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

