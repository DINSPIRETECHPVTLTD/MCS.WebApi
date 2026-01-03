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
    public class OrganizationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrganizationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Organizations
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<Organization>>> GetOrganizations()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null || user.Role != UserRole.Owner)
            {
                return Forbid();
            }

            return await _context.Organizations.ToListAsync();
        }

        // GET: api/Organizations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Organization>> GetOrganization(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            Organization? organization = null;

            if (userType == "Organization")
            {
                if (user.OrgId != id)
                {
                    return Forbid();
                }
                organization = await _context.Organizations.FindAsync(id);
            }

            if (organization == null)
            {
                return NotFound();
            }

            return organization;
        }

        // POST: api/Organizations
        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<Organization>> PostOrganization(Organization organization)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null || user.Role != UserRole.Owner)
            {
                return Forbid();
            }

            organization.CreatedBy = userId;
            organization.CreatedAt = DateTime.UtcNow;
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrganization", new { id = organization.Id }, organization);
        }

        // PUT: api/Organizations/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> PutOrganization(int id, Organization organization)
        {
            if (id != organization.Id)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null || user.Role != UserRole.Owner || user.OrgId != id)
            {
                return Forbid();
            }

            var existingOrg = await _context.Organizations.FindAsync(id);
            if (existingOrg == null)
            {
                return NotFound();
            }

            existingOrg.Name = organization.Name;
            existingOrg.Address1 = organization.Address1;
            existingOrg.Address2 = organization.Address2;
            existingOrg.City = organization.City;
            existingOrg.State = organization.State;
            existingOrg.ZipCode = organization.ZipCode;
            existingOrg.PhoneNumber = organization.PhoneNumber;
            existingOrg.ModifiedBy = userId;
            existingOrg.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Organizations/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null || user.Role != UserRole.Owner || user.OrgId != id)
            {
                return Forbid();
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            organization.IsDeleted = true;
            organization.ModifiedBy = userId;
            organization.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrganizationExists(int id)
        {
            return _context.Organizations.Any(e => e.Id == id);
        }
    }
}

