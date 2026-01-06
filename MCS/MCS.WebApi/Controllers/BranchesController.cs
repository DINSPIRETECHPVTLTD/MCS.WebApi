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

            // Return only Branch properties without navigation properties to avoid circular references
            IQueryable<Branch> query = _context.Branches.AsNoTracking();

            if (userType == "Organization")
            {
                query = query.Where(b => b.OrgId == user.OrgId);
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                query = query.Where(b => b.Id == user.BranchId.Value);
            }
            else
            {
                return Forbid();
            }

            return await query
                .Select(b => new Branch
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address1 = b.Address1,
                    Address2 = b.Address2,
                    City = b.City,
                    State = b.State,
                    Country = b.Country,
                    ZipCode = b.ZipCode,
                    PhoneNumber = b.PhoneNumber,
                    OrgId = b.OrgId,
                    CreatedBy = b.CreatedBy,
                    CreatedAt = b.CreatedAt,
                    ModifiedBy = b.ModifiedBy,
                    ModifiedAt = b.ModifiedAt,
                    IsDeleted = b.IsDeleted
                })
                .ToListAsync();
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

            IQueryable<Branch> query = _context.Branches.AsNoTracking();

            if (userType == "Organization")
            {
                query = query.Where(b => b.Id == id && b.OrgId == user.OrgId);
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || user.BranchId.Value != id)
                {
                    return Forbid();
                }
                query = query.Where(b => b.Id == id);
            }
            else
            {
                return Forbid();
            }

            // Return only Branch properties without navigation properties to avoid circular references
            var branch = await query
                .Select(b => new Branch
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address1 = b.Address1,
                    Address2 = b.Address2,
                    City = b.City,
                    State = b.State,
                    Country = b.Country,
                    ZipCode = b.ZipCode,
                    PhoneNumber = b.PhoneNumber,
                    OrgId = b.OrgId,
                    CreatedBy = b.CreatedBy,
                    CreatedAt = b.CreatedAt,
                    ModifiedBy = b.ModifiedBy,
                    ModifiedAt = b.ModifiedAt,
                    IsDeleted = b.IsDeleted
                })
                .FirstOrDefaultAsync();

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

            // Reload branch without navigation properties to avoid circular references
            var createdBranch = await _context.Branches
                .AsNoTracking()
                .Where(b => b.Id == branch.Id)
                .Select(b => new Branch
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address1 = b.Address1,
                    Address2 = b.Address2,
                    City = b.City,
                    State = b.State,
                    Country = b.Country,
                    ZipCode = b.ZipCode,
                    PhoneNumber = b.PhoneNumber,
                    OrgId = b.OrgId,
                    CreatedBy = b.CreatedBy,
                    CreatedAt = b.CreatedAt,
                    ModifiedBy = b.ModifiedBy,
                    ModifiedAt = b.ModifiedAt,
                    IsDeleted = b.IsDeleted
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction("GetBranch", new { id = branch.Id }, createdBranch);
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

