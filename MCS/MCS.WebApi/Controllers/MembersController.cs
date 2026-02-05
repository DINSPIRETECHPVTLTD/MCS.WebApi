using MCS.WebApi.Data;
using MCS.WebApi.Models;
using MCS.WebApi.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            if (userType == "Organization")
            {
                return await _context.Members
                    .Include(m => m.Center)
                    .ThenInclude(c => c.Branch)
                    .Where(m => m.Center.Branch.OrgId == user.OrgId)
                    .Include(m => m.POC)
                    .ToListAsync();
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                return await _context.Members
                    .Include(m => m.Center)
                    .Where(m => m.Center.BranchId == user.BranchId.Value)
                    .Include(m => m.POC)
                    .ToListAsync();
            }

            return Forbid();
        }
        //GET: api/Members/by Branch
        //No Direct Relation so using Centers(CenterId) -> BranchId
        [HttpGet("branch/{branchId}")]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembersByBranch(int branchId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return Forbid();

            if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || user.BranchId.Value != branchId)
                    return Forbid();
            }
            else if (userType == "Organization")
            {
                var branch = await _context.Branches.FindAsync(branchId);
                if (branch == null || branch.OrgId != user.OrgId)
                    return Forbid();
            }

            var members = await _context.Members
                .Include(m => m.Center)
                .Where(m => m.Center.BranchId == branchId)
                .Include(m => m.POC)
                .ToListAsync();

            return Ok(members);
        }
        // GET: api/Members/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            Member? member = null;

            if (userType == "Organization")
            {
                member = await _context.Members
                    .Include(m => m.Center)
                    .ThenInclude(c => c.Branch)
                    .Include(m => m.POC)
                    .FirstOrDefaultAsync(m => m.Id == id && m.Center.Branch.OrgId == user.OrgId);
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                member = await _context.Members
                    .Include(m => m.Center)
                    .Include(m => m.POC)
                    .FirstOrDefaultAsync(m => m.Id == id && m.Center.BranchId == user.BranchId.Value);
            }

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // POST: api/Members
        [HttpPost]
            [Authorize(Roles = "BranchAdmin,Staff,Owner")]
            public async Task<ActionResult<Member>> PostMember(CreateMemberDto dto)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var member = new Member
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber,
                    DOB = dto.DOB,
                    Age = dto.Age,
                    GuardianFirstName = dto.GuardianFirstName,
                    GuardianLastName = dto.GuardianLastName,
                    GuardianPhone = dto.GuardianPhone,
                    GuardianDOB = dto.GuardianDOB,
                    GuardianAge = dto.GuardianAge,
                    CenterId = dto.CenterId,
                    POCId = dto.POCId,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    MiddleName = dto.MiddleName,
                    AltPhone = dto.AltPhone,
                    Address1 = dto.Address1,
                    Address2 = dto.Address2,
                    City = dto.City,
                    State = dto.State,
                    ZipCode = dto.ZipCode,
                    Aadhaar = dto.Aadhaar,
                    Occupation = dto.Occupation,
                    GuardianMiddleName = dto.GuardianMiddleName,
                };

                _context.Members.Add(member);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetMember", new { id = member.Id }, member);
            }

        
        // PUT: api/Members/5
        [HttpPut("{id}")]
        [Authorize(Roles = "BranchAdmin,Staff")]
        public async Task<IActionResult> PutMember(int id, Member member)
        {
            if (id != member.Id)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return Forbid();
            }

            var existingMember = await _context.Members
                .Include(m => m.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (existingMember == null)
            {
                return NotFound();
            }

            // Validate access
            var userType = User.FindFirst("UserType")!.Value;
            if (userType == "Organization")
            {
                if (existingMember.Center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || existingMember.Center.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            // Validate Center if changed
            if (member.CenterId != existingMember.CenterId)
            {
                var center = await _context.Centers
                    .Include(c => c.Branch)
                    .FirstOrDefaultAsync(c => c.Id == member.CenterId);

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

            // Validate POC if changed
            if (member.POCId != existingMember.POCId)
            {
                var poc = await _context.POCs.FindAsync(member.POCId);
                if (poc == null || poc.CenterId != member.CenterId)
                {
                    return BadRequest("Invalid POC or POC does not belong to the center");
                }
            }

            existingMember.FirstName = member.FirstName;
            existingMember.MiddleName = member.MiddleName;
            existingMember.LastName = member.LastName;
            existingMember.PhoneNumber = member.PhoneNumber;
            existingMember.AltPhone = member.AltPhone;
            existingMember.Address1 = member.Address1;
            existingMember.Address2 = member.Address2;
            existingMember.City = member.City;
            existingMember.State = member.State;
            existingMember.ZipCode = member.ZipCode;
            existingMember.Aadhaar = member.Aadhaar;
            existingMember.DOB = member.DOB;
            existingMember.Age = member.Age;
            existingMember.GuardianFirstName = member.GuardianFirstName;
            existingMember.GuardianMiddleName = member.GuardianMiddleName;
            existingMember.GuardianLastName = member.GuardianLastName;
            existingMember.GuardianPhone = member.GuardianPhone;
            existingMember.GuardianDOB = member.GuardianDOB;
            existingMember.GuardianAge = member.GuardianAge;
            existingMember.CenterId = member.CenterId;
            existingMember.POCId = member.POCId;
            existingMember.ModifiedBy = userId;
            existingMember.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Members/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "BranchAdmin,Staff")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return Forbid();
            }

            var member = await _context.Members
                .Include(m => m.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (member == null)
            {
                return NotFound();
            }

            // Validate access
            var userType = User.FindFirst("UserType")!.Value;
            if (userType == "Organization")
            {
                if (member.Center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || member.Center.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            member.IsDeleted = true;
            member.ModifiedBy = userId;
            member.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

