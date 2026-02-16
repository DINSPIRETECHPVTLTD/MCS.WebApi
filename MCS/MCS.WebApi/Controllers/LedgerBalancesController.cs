using MCS.WebApi.Data;
using Microsoft.AspNetCore.Mvc;
using MCS.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MCS.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LedgerBalancesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LedgerBalancesController(ApplicationDbContext context)
        {
            _context = context;
        }


        // GET: api/<LedgerBalancesController>
        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<Ledger>>> GetAllLedgerBalances()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.Role != UserRole.Owner)
            {
                return Forbid();
            }

            return await _context.Ledgers
                .AsNoTracking()
                .Select(l => new Ledger
                {
                    Id = l.Id,
                    UserId = l.UserId,
                    Amount = l.Amount
                }).ToListAsync();
        }

        // GET api/<LedgerBalancesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<LedgerBalancesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<LedgerBalancesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LedgerBalancesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
