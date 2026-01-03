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
    public class CentersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CentersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Centers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Center>>> GetCenters()
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
                return await _context.Centers
                    .Include(c => c.Branch)
                    .Where(c => c.Branch.OrgId == user.OrgId)
                    .ToListAsync();
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                return await _context.Centers
                    .Where(c => c.BranchId == user.BranchId.Value)
                    .ToListAsync();
            }

            return Forbid();
        }

        // GET: api/Centers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Center>> GetCenter(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            Center? center = null;

            if (userType == "Organization")
            {
                center = await _context.Centers
                    .Include(c => c.Branch)
                    .FirstOrDefaultAsync(c => c.Id == id && c.Branch.OrgId == user.OrgId);
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                center = await _context.Centers
                    .FirstOrDefaultAsync(c => c.Id == id && c.BranchId == user.BranchId.Value);
            }

            if (center == null)
            {
                return NotFound();
            }

            return center;
        }

        // POST: api/Centers
        [HttpPost]
        [Authorize(Roles = "BranchAdmin,Staff")]
        public async Task<ActionResult<Center>> PostCenter(Center center)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return Forbid();
            }

            // Validate Branch belongs to user's organization
            var branch = await _context.Branches.FindAsync(center.BranchId);
            if (branch == null)
            {
                return BadRequest("Invalid branch");
            }

            var userType = User.FindFirst("UserType")!.Value;
            if (userType == "Organization")
            {
                if (branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || branch.Id != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            center.CreatedBy = userId;
            center.CreatedAt = DateTime.UtcNow;
            _context.Centers.Add(center);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCenter", new { id = center.Id }, center);
        }

        // PUT: api/Centers/5
        [HttpPut("{id}")]
        [Authorize(Roles = "BranchAdmin,Staff")]
        public async Task<IActionResult> PutCenter(int id, Center center)
        {
            if (id != center.Id)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return Forbid();
            }

            var existingCenter = await _context.Centers
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingCenter == null)
            {
                return NotFound();
            }

            // Validate access
            var userType = User.FindFirst("UserType")!.Value;
            if (userType == "Organization")
            {
                if (existingCenter.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || existingCenter.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            existingCenter.Name = center.Name;
            existingCenter.ModifiedBy = userId;
            existingCenter.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Centers/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "BranchAdmin,Staff")]
        public async Task<IActionResult> DeleteCenter(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return Forbid();
            }

            var center = await _context.Centers
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (center == null)
            {
                return NotFound();
            }

            // Validate access
            var userType = User.FindFirst("UserType")!.Value;
            if (userType == "Organization")
            {
                if (center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || center.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            center.IsDeleted = true;
            center.ModifiedBy = userId;
            center.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

