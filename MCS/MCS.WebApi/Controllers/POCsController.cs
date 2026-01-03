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
    public class POCsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public POCsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/POCs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<POC>>> GetPOCs()
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
                return await _context.POCs
                    .Include(p => p.Center)
                    .ThenInclude(c => c.Branch)
                    .Where(p => p.Center.Branch.OrgId == user.OrgId)
                    .ToListAsync();
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                return await _context.POCs
                    .Include(p => p.Center)
                    .Where(p => p.Center.BranchId == user.BranchId.Value)
                    .ToListAsync();
            }

            return Forbid();
        }

        // GET: api/POCs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<POC>> GetPOC(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            POC? poc = null;

            if (userType == "Organization")
            {
                poc = await _context.POCs
                    .Include(p => p.Center)
                    .ThenInclude(c => c.Branch)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Center.Branch.OrgId == user.OrgId);
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                poc = await _context.POCs
                    .Include(p => p.Center)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Center.BranchId == user.BranchId.Value);
            }

            if (poc == null)
            {
                return NotFound();
            }

            return poc;
        }

        // GET: api/POCs/Center/5
        [HttpGet("Center/{centerId}")]
        public async Task<ActionResult<IEnumerable<POC>>> GetPOCsByCenter(int centerId)
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
                    .FirstOrDefaultAsync(c => c.Id == centerId && c.Branch.OrgId == user.OrgId);
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                center = await _context.Centers
                    .FirstOrDefaultAsync(c => c.Id == centerId && c.BranchId == user.BranchId.Value);
            }

            if (center == null)
            {
                return NotFound();
            }

            return await _context.POCs
                .Where(p => p.CenterId == centerId)
                .ToListAsync();
        }

        // POST: api/POCs
        [HttpPost]
        [Authorize(Roles = "BranchAdmin,Staff")]
        public async Task<ActionResult<POC>> PostPOC(POC poc)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return Forbid();
            }

            // Validate Center belongs to user's organization/branch
            var center = await _context.Centers
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == poc.CenterId);

            if (center == null)
            {
                return BadRequest("Invalid center");
            }

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

            poc.CreatedBy = userId;
            poc.CreatedAt = DateTime.UtcNow;
            _context.POCs.Add(poc);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPOC", new { id = poc.Id }, poc);
        }

        // PUT: api/POCs/5
        [HttpPut("{id}")]
        [Authorize(Roles = "BranchAdmin,Staff")]
        public async Task<IActionResult> PutPOC(int id, POC poc)
        {
            if (id != poc.Id)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return Forbid();
            }

            var existingPOC = await _context.POCs
                .Include(p => p.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingPOC == null)
            {
                return NotFound();
            }

            // Validate access
            var userType = User.FindFirst("UserType")!.Value;
            if (userType == "Organization")
            {
                if (existingPOC.Center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || existingPOC.Center.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            // Validate Center if changed
            if (poc.CenterId != existingPOC.CenterId)
            {
                var center = await _context.Centers
                    .Include(c => c.Branch)
                    .FirstOrDefaultAsync(c => c.Id == poc.CenterId);

                if (center == null)
                {
                    return BadRequest("Invalid center");
                }

                if (userType == "Organization" && center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
                else if (userType == "Branch" && (!user.BranchId.HasValue || center.BranchId != user.BranchId.Value))
                {
                    return Forbid();
                }
            }

            existingPOC.FirstName = poc.FirstName;
            existingPOC.MiddleName = poc.MiddleName;
            existingPOC.LastName = poc.LastName;
            existingPOC.PhoneNumber = poc.PhoneNumber;
            existingPOC.AltPhone = poc.AltPhone;
            existingPOC.Address1 = poc.Address1;
            existingPOC.Address2 = poc.Address2;
            existingPOC.City = poc.City;
            existingPOC.State = poc.State;
            existingPOC.ZipCode = poc.ZipCode;
            existingPOC.Aadhaar = poc.Aadhaar;
            existingPOC.DOB = poc.DOB;
            existingPOC.Age = poc.Age;
            existingPOC.CenterId = poc.CenterId;
            existingPOC.ModifiedBy = userId;
            existingPOC.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/POCs/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "BranchAdmin,Staff")]
        public async Task<IActionResult> DeletePOC(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return Forbid();
            }

            var poc = await _context.POCs
                .Include(p => p.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (poc == null)
            {
                return NotFound();
            }

            // Validate access
            var userType = User.FindFirst("UserType")!.Value;
            if (userType == "Organization")
            {
                if (poc.Center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || poc.Center.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            poc.IsDeleted = true;
            poc.ModifiedBy = userId;
            poc.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

