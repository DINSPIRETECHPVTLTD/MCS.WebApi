using MCS.WebApi.Data;
using Microsoft.AspNetCore.Mvc;
using MCS.WebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MCS.WebApi.Services;
using MCS.WebApi.DTOs;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MCS.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LedgerBalancesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LedgerTransactionService _ledgerTransactionService;

        public LedgerBalancesController(ApplicationDbContext context, LedgerTransactionService ledgerTransactionService)
        {
            _context = context;
            _ledgerTransactionService = ledgerTransactionService;
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

        [HttpGet("user-transactions/{id}")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<LedgerTransaction>>> GetAllUserTransactions(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.Role != UserRole.Owner)
            {
                return Forbid();
            }

            return await _context.LedgerTransactions
                .AsNoTracking()
                .Where(l => l.PaidFromUserId == id || l.PaidToUserId == id)
                .OrderByDescending(l => l.PaymentDate)
                .Select(l => new LedgerTransaction
                {
                    Id = l.Id,
                    PaidFromUserId = l.PaidFromUserId,
                    PaidToUserId = l.PaidToUserId,
                    TransactionType = l.TransactionType,
                    PaymentDate = l.PaymentDate,
                    CreatedBy = l.CreatedBy,
                    Amount = l.Amount,
                    CreatedDate = l.CreatedDate
                }).ToListAsync();
        }

        // GET api/<LedgerBalancesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        [HttpPost("fund-transfer")]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<FundTransferDto>>> CreateFundTransfer(FundTransferDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser == null || currentUser.Role != UserRole.Owner)
            {
                return Forbid();
            }

            if (dto.PaidFromUserId >0 && dto.PaidToUserId > 0)
            {

                var paidFrom = await _context.Users.FindAsync(dto.PaidFromUserId);

                var paidTo = await _context.Users.FindAsync(dto.PaidToUserId);

                string comment = $"Fund Transfer of {dto.Amount} from {paidFrom?.FirstName} {paidFrom?.LastName} to {paidTo?.FirstName} {paidTo?.LastName}";

                var ledgerTransaction = await _ledgerTransactionService.RecordTransferAsync(
                    dto.PaidFromUserId,
                    dto.PaidToUserId,
                    dto.Amount,
                    null,
                    comment,
                    userId
                );

                _context.SaveChanges();

                var resultDto = new FundTransferDto
                {
                    PaidFromUserId = dto.PaidFromUserId,
                    PaidToUserId = dto.PaidToUserId,
                    Amount = dto.Amount,
                    TransferDate = dto.TransferDate
                    // Add LedgerTransactionId if you extend the DTO
                };

                // Return as a single-item list to match the return type
                return CreatedAtAction(nameof(CreateFundTransfer), new { id = ledgerTransaction.Id }, resultDto );

            }
            else
            {
                return Forbid();
            }
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
