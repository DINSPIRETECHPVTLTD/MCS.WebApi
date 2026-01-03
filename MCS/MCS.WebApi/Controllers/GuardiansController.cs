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
    public class GuardiansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GuardiansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Guardians
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Guardian>>> GetGuardians()
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
                return await _context.Guardians
                    .Include(g => g.Member)
                    .ThenInclude(m => m.Branch)
                    .Where(g => g.Member.Branch.OrganizationId == orgUser.OrganizationId)
                    .ToListAsync();
            }
            else if (userType == "Branch")
            {
                var branchUser = await _context.BranchUsers.FindAsync(userId);
                if (branchUser == null)
                {
                    return Forbid();
                }
                return await _context.Guardians
                    .Include(g => g.Member)
                    .Where(g => g.Member.BranchId == branchUser.BranchId)
                    .ToListAsync();
            }

            return Forbid();
        }

        // GET: api/Guardians/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Guardian>> GetGuardian(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;

            Guardian? guardian = null;

            if (userType == "Organization")
            {
                var orgUser = await _context.OrganizationUsers.FindAsync(userId);
                if (orgUser == null)
                {
                    return Forbid();
                }
                guardian = await _context.Guardians
                    .Include(g => g.Member)
                    .ThenInclude(m => m.Branch)
                    .FirstOrDefaultAsync(g => g.GuardianId == id && g.Member.Branch.OrganizationId == orgUser.OrganizationId);
            }
            else if (userType == "Branch")
            {
                var branchUser = await _context.BranchUsers.FindAsync(userId);
                if (branchUser == null)
                {
                    return Forbid();
                }
                guardian = await _context.Guardians
                    .Include(g => g.Member)
                    .FirstOrDefaultAsync(g => g.GuardianId == id && g.Member.BranchId == branchUser.BranchId);
            }

            if (guardian == null)
            {
                return NotFound();
            }

            return guardian;
        }

        // GET: api/Guardians/Member/5
        [HttpGet("Member/{memberId}")]
        public async Task<ActionResult<IEnumerable<Guardian>>> GetGuardiansByMember(int memberId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;

            Member? member = null;

            if (userType == "Organization")
            {
                var orgUser = await _context.OrganizationUsers.FindAsync(userId);
                if (orgUser == null)
                {
                    return Forbid();
                }
                member = await _context.Members
                    .Include(m => m.Branch)
                    .FirstOrDefaultAsync(m => m.MemberId == memberId && m.Branch.OrganizationId == orgUser.OrganizationId);
            }
            else if (userType == "Branch")
            {
                var branchUser = await _context.BranchUsers.FindAsync(userId);
                if (branchUser == null)
                {
                    return Forbid();
                }
                member = await _context.Members
                    .FirstOrDefaultAsync(m => m.MemberId == memberId && m.BranchId == branchUser.BranchId);
            }

            if (member == null)
            {
                return NotFound();
            }

            return await _context.Guardians
                .Where(g => g.MemberId == memberId)
                .ToListAsync();
        }

        // POST: api/Guardians
        [HttpPost]
        [Authorize(Roles = "BranchUser")]
        public async Task<ActionResult<Guardian>> PostGuardian(Guardian guardian)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var branchUser = await _context.BranchUsers.FindAsync(userId);
            
            if (branchUser == null)
            {
                return Forbid();
            }

            var member = await _context.Members.FindAsync(guardian.MemberId);
            if (member == null || member.BranchId != branchUser.BranchId)
            {
                return BadRequest("Invalid member");
            }

            guardian.CreatedDate = DateTime.UtcNow;
            _context.Guardians.Add(guardian);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGuardian", new { id = guardian.GuardianId }, guardian);
        }

        // PUT: api/Guardians/5
        [HttpPut("{id}")]
        [Authorize(Roles = "BranchUser")]
        public async Task<IActionResult> PutGuardian(int id, Guardian guardian)
        {
            if (id != guardian.GuardianId)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var branchUser = await _context.BranchUsers.FindAsync(userId);
            
            if (branchUser == null || branchUser.Role != BranchRole.BranchUser)
            {
                return Forbid();
            }

            var existingGuardian = await _context.Guardians
                .Include(g => g.Member)
                .FirstOrDefaultAsync(g => g.GuardianId == id && g.Member.BranchId == branchUser.BranchId);

            if (existingGuardian == null)
            {
                return NotFound();
            }

            existingGuardian.FirstName = guardian.FirstName;
            existingGuardian.MiddleName = guardian.MiddleName;
            existingGuardian.LastName = guardian.LastName;
            existingGuardian.DOB = guardian.DOB;
            existingGuardian.Age = guardian.Age;
            existingGuardian.Phone = guardian.Phone;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Guardians/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "BranchUser")]
        public async Task<IActionResult> DeleteGuardian(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var branchUser = await _context.BranchUsers.FindAsync(userId);
            
            if (branchUser == null || branchUser.Role != BranchRole.BranchUser)
            {
                return Forbid();
            }

            var guardian = await _context.Guardians
                .Include(g => g.Member)
                .FirstOrDefaultAsync(g => g.GuardianId == id && g.Member.BranchId == branchUser.BranchId);

            if (guardian == null)
            {
                return NotFound();
            }

            _context.Guardians.Remove(guardian);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

