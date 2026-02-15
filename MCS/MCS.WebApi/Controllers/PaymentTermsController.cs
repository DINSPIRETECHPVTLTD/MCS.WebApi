using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MCS.WebApi.Data;
using MCS.WebApi.DTOs;
using MCS.WebApi.Models;
using System.Security.Claims;

namespace MCS.WebApi.Controllers
{
    [ApiController]
    [Route("api/paymentterms")]
    [Authorize]
    public class PaymentTermsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentTermsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/PaymentTerms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentTermResponseDto>>> GetPaymentTerms()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Forbid();

            var list = await _context.PaymentTerms
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.PaymentTermId)
                .ToListAsync();
            return Ok(list.Select(MapToDto));
        }

        // GET: api/PaymentTerms/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentTermResponseDto>> GetPaymentTerm(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Forbid();

            var entity = await _context.PaymentTerms
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PaymentTermId == id && !p.IsDeleted);
            if (entity == null)
                return NotFound();
            return Ok(MapToDto(entity));
        }

        // POST: api/PaymentTerms
        [HttpPost]
        [Authorize(Roles = "Owner,BranchAdmin,Staff")]
        public async Task<ActionResult<PaymentTermResponseDto>> PostPaymentTerm([FromBody] CreatePaymentTermDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Forbid();

            if (dto == null)
                return BadRequest(new { message = "Request body is required." });

            var entity = new PaymentTerm
            {
                PaymentTermName = dto.PaymentTerm ?? "",
                PaymentType = dto.PaymentType ?? "",
                NoOfTerms = dto.NoOfTerms,
                ProcessingFee = dto.ProcessingFee,
                RateOfInterest = dto.RateOfInterest,
                InsuranceFee = dto.InsuranceFee,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.PaymentTerms.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPaymentTerm), new { id = entity.PaymentTermId }, MapToDto(entity));
        }

        // PUT: api/PaymentTerms/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Owner,BranchAdmin,Staff")]
        public async Task<IActionResult> PutPaymentTerm(int id, [FromBody] CreatePaymentTermDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Forbid();

            if (dto == null)
                return BadRequest(new { message = "Request body is required." });

            var entity = await _context.PaymentTerms.FindAsync(id);
            if (entity == null)
                return NotFound();
            if (entity.IsDeleted)
                return NotFound(new { message = "Cannot update a deleted payment term." });

            entity.PaymentTermName = dto.PaymentTerm ?? entity.PaymentTermName;
            entity.PaymentType = dto.PaymentType ?? entity.PaymentType;
            entity.NoOfTerms = dto.NoOfTerms;
            entity.ProcessingFee = dto.ProcessingFee;
            entity.RateOfInterest = dto.RateOfInterest;
            entity.InsuranceFee = dto.InsuranceFee;
            entity.ModifiedBy = userId;
            entity.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/PaymentTerms/5 (soft delete)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner,BranchAdmin,Staff")]
        public async Task<IActionResult> DeletePaymentTerm(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Forbid();

            var entity = await _context.PaymentTerms.FindAsync(id);
            if (entity == null)
                return NotFound();

            entity.IsDeleted = true;
            entity.ModifiedBy = userId;
            entity.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static PaymentTermResponseDto MapToDto(PaymentTerm e)
        {
            return new PaymentTermResponseDto
            {
                PaymentTermId = e.PaymentTermId,
                PaymentTerm = e.PaymentTermName,
                PaymentType = e.PaymentType,
                NoOfTerms = e.NoOfTerms,
                ProcessingFee = e.ProcessingFee,
                RateOfInterest = e.RateOfInterest,
                InsuranceFee = e.InsuranceFee,
                IsDeleted = e.IsDeleted
            };
        }
    }
}
