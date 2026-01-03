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
    public class MembersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
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
                return await _context.Members
                    .Include(m => m.Branch)
                    .Where(m => m.Branch.OrganizationId == orgUser.OrganizationId)
                    .Include(m => m.Center)
                    .ToListAsync();
            }
            else if (userType == "Branch")
            {
                var branchUser = await _context.BranchUsers.FindAsync(userId);
                if (branchUser == null)
                {
                    return Forbid();
                }
                return await _context.Members
                    .Where(m => m.BranchId == branchUser.BranchId)
                    .Include(m => m.Center)
                    .ToListAsync();
            }

            return Forbid();
        }

        // GET: api/Members/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(int id)
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
                    .Include(m => m.Center)
                    .FirstOrDefaultAsync(m => m.MemberId == id && m.Branch.OrganizationId == orgUser.OrganizationId);
            }
            else if (userType == "Branch")
            {
                var branchUser = await _context.BranchUsers.FindAsync(userId);
                if (branchUser == null)
                {
                    return Forbid();
                }
                member = await _context.Members
                    .Include(m => m.Center)
                    .FirstOrDefaultAsync(m => m.MemberId == id && m.BranchId == branchUser.BranchId);
            }

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // POST: api/Members
        [HttpPost]
        [Authorize(Roles = "BranchUser,Staff")]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var branchUser = await _context.BranchUsers.FindAsync(userId);
            
            if (branchUser == null)
            {
                return Forbid();
            }

            member.BranchId = branchUser.BranchId;
            
            // Only BranchUser can assign center
            if (branchUser.Role != BranchRole.BranchUser)
            {
                member.CenterId = null;
            }

            member.CreatedDate = DateTime.UtcNow;
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMember", new { id = member.MemberId }, member);
        }

        // PUT: api/Members/5
        [HttpPut("{id}")]
        [Authorize(Roles = "BranchUser")]
        public async Task<IActionResult> PutMember(int id, Member member)
        {
            if (id != member.MemberId)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var branchUser = await _context.BranchUsers.FindAsync(userId);
            
            if (branchUser == null || branchUser.Role != BranchRole.BranchUser)
            {
                return Forbid();
            }

            var existingMember = await _context.Members.FindAsync(id);
            if (existingMember == null || existingMember.BranchId != branchUser.BranchId)
            {
                return NotFound();
            }

            existingMember.FirstName = member.FirstName;
            existingMember.MiddleName = member.MiddleName;
            existingMember.LastName = member.LastName;
            existingMember.DOB = member.DOB;
            existingMember.Age = member.Age;
            existingMember.Phone = member.Phone;
            existingMember.Address = member.Address;
            existingMember.Aadhaar = member.Aadhaar;
            existingMember.Occupation = member.Occupation;
            existingMember.CenterId = member.CenterId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Members/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "BranchUser")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var branchUser = await _context.BranchUsers.FindAsync(userId);
            
            if (branchUser == null || branchUser.Role != BranchRole.BranchUser)
            {
                return Forbid();
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null || member.BranchId != branchUser.BranchId)
            {
                return NotFound();
            }

            member.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

