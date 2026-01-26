using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MCS.WebApi.Data;
using MCS.WebApi.Models;
using MCS.WebApi.DTOs;
using System.Security.Claims;

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
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
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
                    .Where(p => p.Center.Branch.OrgId == user.OrgId && !p.IsDeleted)
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
                    .Where(p => p.Center.BranchId == user.BranchId.Value && !p.IsDeleted)
                    .ToListAsync();
            }

            return Forbid();
        }

        // GET: api/POCs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<POC>> GetPOC(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
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
                    .FirstOrDefaultAsync(p => p.Id == id && p.Center.Branch.OrgId == user.OrgId && !p.IsDeleted);
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                poc = await _context.POCs
                    .Include(p => p.Center)
                    .FirstOrDefaultAsync(p => p.Id == id && p.Center.BranchId == user.BranchId.Value && !p.IsDeleted);
            }

            if (poc == null)
            {
                return NotFound();
            }

            return poc;
        }

        // GET: api/POCs/center/{centerId}
        [HttpGet("center/{centerId}")]
        public async Task<ActionResult<IEnumerable<POC>>> GetPOCsByCenter(int centerId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            var center = await _context.Centers
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == centerId);

            if (center == null)
            {
                return NotFound("Center not found");
            }

            // Validate access
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

            return await _context.POCs
                .Where(p => p.CenterId == centerId && !p.IsDeleted)
                .ToListAsync();
        }

        // POST: api/POCs
        [HttpPost]
        [Authorize(Roles = "BranchAdmin,Staff,Owner")]
        public async Task<ActionResult<POC>> PostPOC(CreatePOCDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            // Validate center access
            var center = await _context.Centers
                .Include(c => c.Branch)
                .FirstOrDefaultAsync(c => c.Id == dto.CenterId);

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

            var poc = new POC
            {
                FirstName = dto.FirstName,
                MiddleName = dto.MiddleName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                AltPhone = dto.AltPhone,
                Address1 = dto.Address1,
                Address2 = dto.Address2,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode,
                CenterId = dto.CenterId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

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

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            var existingPOC = await _context.POCs
                .Include(p => p.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

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
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            var poc = await _context.POCs
                .Include(p => p.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

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

            // Check if POC is referenced by any members
            var hasMembers = await _context.Members.AnyAsync(m => m.POCId == id && !m.IsDeleted);
            if (hasMembers)
            {
                return BadRequest("Cannot delete POC as it is assigned to active members");
            }

            poc.IsDeleted = true;
            poc.ModifiedBy = userId;
            poc.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

