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

            if (userType == "Organization")
            {
                var orgUser = await _context.OrganizationUsers.FindAsync(userId);
                if (orgUser == null)
                {
                    return Forbid();
                }
                return await _context.Centers
                    .Include(c => c.Branch)
                    .Where(c => c.Branch.OrganizationId == orgUser.OrganizationId)
                    .ToListAsync();
            }
            else if (userType == "Branch")
            {
                var branchUser = await _context.BranchUsers.FindAsync(userId);
                if (branchUser == null)
                {
                    return Forbid();
                }
                return await _context.Centers
                    .Where(c => c.BranchId == branchUser.BranchId)
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

            Center? center = null;

            if (userType == "Organization")
            {
                var orgUser = await _context.OrganizationUsers.FindAsync(userId);
                if (orgUser == null)
                {
                    return Forbid();
                }
                center = await _context.Centers
                    .Include(c => c.Branch)
                    .FirstOrDefaultAsync(c => c.CenterId == id && c.Branch.OrganizationId == orgUser.OrganizationId);
            }
            else if (userType == "Branch")
            {
                var branchUser = await _context.BranchUsers.FindAsync(userId);
                if (branchUser == null)
                {
                    return Forbid();
                }
                center = await _context.Centers
                    .FirstOrDefaultAsync(c => c.CenterId == id && c.BranchId == branchUser.BranchId);
            }

            if (center == null)
            {
                return NotFound();
            }

            return center;
        }

        // POST: api/Centers
        [HttpPost]
        [Authorize(Roles = "BranchUser")]
        public async Task<ActionResult<Center>> PostCenter(Center center)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var branchUser = await _context.BranchUsers.FindAsync(userId);
            
            if (branchUser == null || branchUser.Role != BranchRole.BranchUser)
            {
                return Forbid();
            }

            center.BranchId = branchUser.BranchId;
            center.CreatedDate = DateTime.UtcNow;
            _context.Centers.Add(center);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCenter", new { id = center.CenterId }, center);
        }

        // PUT: api/Centers/5
        [HttpPut("{id}")]
        [Authorize(Roles = "BranchUser")]
        public async Task<IActionResult> PutCenter(int id, Center center)
        {
            if (id != center.CenterId)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var branchUser = await _context.BranchUsers.FindAsync(userId);
            
            if (branchUser == null || branchUser.Role != BranchRole.BranchUser)
            {
                return Forbid();
            }

            var existingCenter = await _context.Centers.FindAsync(id);
            if (existingCenter == null || existingCenter.BranchId != branchUser.BranchId)
            {
                return NotFound();
            }

            existingCenter.Name = center.Name;
            existingCenter.Description = center.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Centers/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "BranchUser")]
        public async Task<IActionResult> DeleteCenter(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var branchUser = await _context.BranchUsers.FindAsync(userId);
            
            if (branchUser == null || branchUser.Role != BranchRole.BranchUser)
            {
                return Forbid();
            }

            var center = await _context.Centers.FindAsync(id);
            if (center == null || center.BranchId != branchUser.BranchId)
            {
                return NotFound();
            }

            center.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

