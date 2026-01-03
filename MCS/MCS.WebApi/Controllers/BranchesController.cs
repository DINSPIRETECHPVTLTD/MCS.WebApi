using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MCS.WebApi.Data;
using MCS.WebApi.Models;

namespace MCS.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BranchesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BranchesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Branches
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Branch>>> GetBranches()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            if (userType == "Organization")
            {
                return await _context.Branches
                    .Where(b => b.OrgId == user.OrgId)
                    .ToListAsync();
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                return await _context.Branches
                    .Where(b => b.Id == user.BranchId.Value)
                    .ToListAsync();
            }

            return Forbid();
        }

        // GET: api/Branches/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Branch>> GetBranch(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            Branch? branch = null;

            if (userType == "Organization")
            {
                branch = await _context.Branches
                    .FirstOrDefaultAsync(b => b.Id == id && b.OrgId == user.OrgId);
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || user.BranchId.Value != id)
                {
                    return Forbid();
                }
                branch = await _context.Branches.FindAsync(id);
            }

            if (branch == null)
            {
                return NotFound();
            }

            return branch;
        }

        // POST: api/Branches
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<Branch>> PostBranch(Branch branch)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null || user.Role != UserRole.Owner)
            {
                return Forbid();
            }

            branch.OrgId = user.OrgId;
            branch.CreatedBy = userId;
            branch.CreatedAt = DateTime.UtcNow;
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBranch", new { id = branch.Id }, branch);
        }

        // PUT: api/Branches/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> PutBranch(int id, Branch branch)
        {
            if (id != branch.Id)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null || user.Role != UserRole.Owner)
            {
                return Forbid();
            }

            var existingBranch = await _context.Branches.FindAsync(id);
            if (existingBranch == null || existingBranch.OrgId != user.OrgId)
            {
                return NotFound();
            }

            existingBranch.Name = branch.Name;
            existingBranch.Address1 = branch.Address1;
            existingBranch.Address2 = branch.Address2;
            existingBranch.City = branch.City;
            existingBranch.State = branch.State;
            existingBranch.Country = branch.Country;
            existingBranch.ZipCode = branch.ZipCode;
            existingBranch.PhoneNumber = branch.PhoneNumber;
            existingBranch.ModifiedBy = userId;
            existingBranch.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Branches/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null || user.Role != UserRole.Owner)
            {
                return Forbid();
            }

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null || branch.OrgId != user.OrgId)
            {
                return NotFound();
            }

            branch.IsDeleted = true;
            branch.ModifiedBy = userId;
            branch.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

