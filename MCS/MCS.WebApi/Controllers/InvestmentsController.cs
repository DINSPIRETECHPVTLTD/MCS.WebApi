using MCS.WebApi.Data;
using MCS.WebApi.DTOs;
using MCS.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MCS.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvestmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InvestmentsController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: api/<InvestmentsController>
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<Investment>>> GetInvestments()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.Role != UserRole.Owner)
            {
                return Forbid();
            }

            // Return only User properties without navigation properties to avoid circular references
            return await _context.Investments
                .AsNoTracking()
                .Select(i => new Investment
                {
                    Id = i.Id,
                    UserId = i.UserId,
                    Amount = i.Amount,
                    InvestmentDate = i.InvestmentDate,
                    CreatedById = i.CreatedById,
                    CreatedDate = i.CreatedDate
                }).ToListAsync();
        }

        // GET api/<InvestmentsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<InvestmentsController>
        [HttpPost]
        public async Task<ActionResult<Investment>> PostInvestment(InvestorDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser == null || currentUser.Role != UserRole.Owner)
            {
                return Forbid();
            }

            var investment = new Investment
            {
                UserId = dto.UserId,
                Amount = dto.Amount,
                InvestmentDate = dto.InvestmentDate,
                CreatedById = userId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Investments.Add(investment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = investment.Id }, investment);


        }

        // PUT api/<InvestmentsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<InvestmentsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
