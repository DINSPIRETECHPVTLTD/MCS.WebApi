using MCS.WebApi.Data;
using MCS.WebApi.DTOs;
using MCS.WebApi.Models;
using MCS.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MCS.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpensesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LedgerTransactionService _ledgerTransactionService;

        public ExpensesController(ApplicationDbContext context, LedgerTransactionService ledgerTransactionService)
        {
            _context = context;
            _ledgerTransactionService = ledgerTransactionService;
        }


        [HttpGet]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<LedgerTransaction>>> GetExpenses()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.Role != UserRole.Owner)
            {
                return Forbid();
            }

            return await _context.LedgerTransactions
                .AsNoTracking()
                .Where(e => e.TransactionType == "Expense")
                .OrderByDescending(e => e.PaymentDate)
                .Select(e => new LedgerTransaction
                {
                    Id = e.Id,
                    PaidFromUserId = e.PaidFromUserId,
                    TransactionType = e.TransactionType,
                    PaymentDate = e.PaymentDate,
                    CreatedBy = e.CreatedBy,
                    Amount = e.Amount,
                    Comments = e.Comments,
                    CreatedDate = e.CreatedDate
                }).ToListAsync();
        }

        [HttpPost]
        [Authorize(Roles = "Owner")]
        public async Task<ActionResult<IEnumerable<ExpenseDto>>> CreateExpense(ExpenseDto dto)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var currentUser = await _context.Users.FindAsync(userId);

            if (currentUser == null || currentUser.Role != UserRole.Owner)
            {
                return Forbid();
            }

            string transactionType = "Expense";

            if (dto.UserId > 0)
            {

                var paidFrom = await _context.Users.FindAsync(dto.UserId);

                var ledgerTransaction = await _ledgerTransactionService.RecordExpenseAsync(
                    dto.UserId,
                    dto.Amount,
                    transactionType,
                    null,
                    dto.Comment,
                    userId
                );

                _context.SaveChanges();

                var resultDto = new ExpenseDto
                {
                    UserId = dto.UserId,
                    Amount = dto.Amount,
                    ExpenseDate = dto.ExpenseDate,
                    Comment = dto.Comment

                };

                // Return as a single-item list to match the return type
                return CreatedAtAction(nameof(CreateExpense), new { id = ledgerTransaction.Id }, resultDto);

            }
            else
            {
                return Forbid();
            }
        }
    }
}
