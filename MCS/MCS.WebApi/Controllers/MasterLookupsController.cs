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
    public class MasterLookupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MasterLookupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET: api/MasterLookups
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MasterLookup>>> GetMasterLookups(
            [FromQuery] string? lookupKey = null,
            [FromQuery] bool? isActive = null)
        {
            var query = _context.MasterLookups.AsNoTracking();

            if (!string.IsNullOrEmpty(lookupKey))
                query = query.Where(m => m.LookupKey == lookupKey);
            if (isActive.HasValue)
                query = query.Where(m => m.IsActive == isActive.Value);

            return await query.OrderBy(m => m.LookupKey).ThenBy(m => m.SortOrder).ToListAsync();
        }

        /// <summary>
        /// GET: api/MasterLookups/5
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MasterLookup>> GetMasterLookup(int id)
        {
            var item = await _context.MasterLookups.FindAsync(id);
            if (item == null)
                return NotFound();
            return item;
        }

        /// <summary>
        /// POST: api/MasterLookups
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<MasterLookup>> PostMasterLookup([FromBody] CreateMasterLookupDto dto)
        {
            var exists = await _context.MasterLookups
                .AnyAsync(m => m.LookupKey == dto.LookupKey && m.LookupCode == dto.LookupCode);
            if (exists)
                return Conflict(new { message = "A lookup with this LookupKey and LookupCode already exists." });

            var createdBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value;

            var entity = new MasterLookup
            {
                LookupKey = dto.LookupKey,
                LookupCode = dto.LookupCode,
                LookupValue = dto.LookupValue,
                NumericValue = dto.NumericValue,
                SortOrder = dto.SortOrder,
                IsActive = dto.IsActive,
                Description = dto.Description,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.MasterLookups.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMasterLookup), new { id = entity.Id }, entity);
        }

        /// <summary>
        /// PUT: api/MasterLookups/5
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMasterLookup(int id, [FromBody] CreateMasterLookupDto dto)
        {
            var entity = await _context.MasterLookups.FindAsync(id);
            if (entity == null)
                return NotFound();

            var duplicate = await _context.MasterLookups
                .AnyAsync(m => m.LookupKey == dto.LookupKey && m.LookupCode == dto.LookupCode && m.Id != id);
            if (duplicate)
                return Conflict(new { message = "Another lookup with this LookupKey and LookupCode already exists." });

            var updatedBy = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(ClaimTypes.Email)?.Value;

            entity.LookupKey = dto.LookupKey;
            entity.LookupCode = dto.LookupCode;
            entity.LookupValue = dto.LookupValue;
            entity.NumericValue = dto.NumericValue;
            entity.SortOrder = dto.SortOrder;
            entity.IsActive = dto.IsActive;
            entity.Description = dto.Description;
            entity.UpdatedOn = DateTime.UtcNow;
            entity.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// DELETE: api/MasterLookups/5
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMasterLookup(int id)
        {
            var entity = await _context.MasterLookups.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.MasterLookups.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
