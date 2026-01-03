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

            if (userType == "Organization")
            {
                var orgUser = await _context.OrganizationUsers.FindAsync(userId);
                if (orgUser == null)
                {
                    return Forbid();
                }
                return await _context.Branches
                    .Where(b => b.OrganizationId == orgUser.OrganizationId)
                    .ToListAsync();
            }
            else if (userType == "Branch")
            {
                var branchUser = await _context.BranchUsers.FindAsync(userId);
                if (branchUser == null)
                {
                    return Forbid();
                }
                return await _context.Branches
                    .Where(b => b.BranchId == branchUser.BranchId)
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

            Branch? branch = null;

            if (userType == "Organization")
            {
                var orgUser = await _context.OrganizationUsers.FindAsync(userId);
                if (orgUser == null)
                {
                    return Forbid();
                }
                branch = await _context.Branches
                    .FirstOrDefaultAsync(b => b.BranchId == id && b.OrganizationId == orgUser.OrganizationId);
            }
            else if (userType == "Branch")
            {
                var branchUser = await _context.BranchUsers.FindAsync(userId);
                if (branchUser == null || branchUser.BranchId != id)
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
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
            {
                return Forbid();
            }

            branch.OrganizationId = orgUser.OrganizationId;
            branch.CreatedDate = DateTime.UtcNow;
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBranch", new { id = branch.BranchId }, branch);
        }

        // PUT: api/Branches/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> PutBranch(int id, Branch branch)
        {
            if (id != branch.BranchId)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
            {
                return Forbid();
            }

            var existingBranch = await _context.Branches.FindAsync(id);
            if (existingBranch == null || existingBranch.OrganizationId != orgUser.OrganizationId)
            {
                return NotFound();
            }

            existingBranch.Name = branch.Name;
            existingBranch.Address = branch.Address;
            existingBranch.Phone = branch.Phone;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Branches/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteBranch(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
            {
                return Forbid();
            }

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null || branch.OrganizationId != orgUser.OrganizationId)
            {
                return NotFound();
            }

            branch.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

