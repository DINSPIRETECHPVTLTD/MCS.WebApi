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
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner)
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

            Organization? organization = null;

            if (userType == "Organization")
            {
                var orgUser = await _context.OrganizationUsers.FindAsync(userId);
                if (orgUser == null || orgUser.OrganizationId != id)
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
            organization.CreatedDate = DateTime.UtcNow;
            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrganization", new { id = organization.OrganizationId }, organization);
        }

        // PUT: api/Organizations/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> PutOrganization(int id, Organization organization)
        {
            if (id != organization.OrganizationId)
            {
                return BadRequest();
            }

            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner || orgUser.OrganizationId != id)
            {
                return Forbid();
            }

            _context.Entry(organization).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrganizationExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Organizations/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var orgUser = await _context.OrganizationUsers.FindAsync(userId);
            
            if (orgUser == null || orgUser.Role != OrganizationRole.Owner || orgUser.OrganizationId != id)
            {
                return Forbid();
            }

            var organization = await _context.Organizations.FindAsync(id);
            if (organization == null)
            {
                return NotFound();
            }

            organization.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrganizationExists(int id)
        {
            return _context.Organizations.Any(e => e.OrganizationId == id);
        }
    }
}

