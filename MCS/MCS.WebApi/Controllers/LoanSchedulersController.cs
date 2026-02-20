using MCS.WebApi.Data;
using MCS.WebApi.Models;
using MCS.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MCS.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LoanSchedulersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LoanSchedulerService _loanSchedulerService;

        public LoanSchedulersController(ApplicationDbContext context, LoanSchedulerService loanSchedulerService)
        {
            _context = context;
            _loanSchedulerService = loanSchedulerService;
        }

        // POST: api/LoanSchedulers/generate/{loanId}
        [HttpPost("generate/{loanId}")]
        [Authorize(Roles = "BranchAdmin,Staff,Owner")]
        public async Task<ActionResult<IEnumerable<LoanScheduler>>> GenerateEmiSchedule(int loanId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            // Get the loan with member details for access validation
            var loan = await _context.Loans
                .Include(l => l.Member)
                .ThenInclude(m => m.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(l => l.Id == loanId && !l.IsDeleted);

            if (loan == null)
            {
                return NotFound("Loan not found");
            }

            // Check access based on user type
            if (userType == "Organization")
            {
                if (loan.Member.Center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || loan.Member.Center.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

                // Check if schedules already exist for this loan
                var existingSchedules = await _context.LoanSchedulers
                    .Where(ls => ls.LoanId == loanId)
                    .ToListAsync();

                if (existingSchedules.Any())
                {
                    return BadRequest("EMI schedule already exists for this loan. Delete existing schedules first.");
                }

                // Use the service to generate schedules
                try
                {
                    var schedules = await _loanSchedulerService.GenerateEmiScheduleAsync(loanId, userId);
                    return Ok(schedules);
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(ex.Message);
                }
            }

        // GET: api/LoanSchedulers/loan/{loanId}
        [HttpGet("loan/{loanId}")]
        public async Task<ActionResult<IEnumerable<LoanScheduler>>> GetSchedulesByLoan(int loanId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            // Get the loan with member details for access validation
            var loan = await _context.Loans
                .Include(l => l.Member)
                .ThenInclude(m => m.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(l => l.Id == loanId && !l.IsDeleted);

            if (loan == null)
            {
                return NotFound("Loan not found");
            }

            // Check access based on user type
            if (userType == "Organization")
            {
                if (loan.Member.Center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || loan.Member.Center.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            var schedules = await _context.LoanSchedulers
                .Where(ls => ls.LoanId == loanId)
                .OrderBy(ls => ls.InstallmentNo)
                .ToListAsync();

            return Ok(schedules);
        }

        // GET: api/LoanSchedulers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LoanScheduler>> GetSchedule(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            var schedule = await _context.LoanSchedulers
                .Include(ls => ls.Loan)
                .ThenInclude(l => l.Member)
                .ThenInclude(m => m.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(ls => ls.LoanSchedulerId == id);

            if (schedule == null)
            {
                return NotFound("Schedule not found");
            }

            // Check access based on user type
            if (userType == "Organization")
            {
                if (schedule.Loan.Member.Center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || schedule.Loan.Member.Center.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            return Ok(schedule);
        }

        // DELETE: api/LoanSchedulers/loan/{loanId}
        [HttpDelete("loan/{loanId}")]
        [Authorize(Roles = "BranchAdmin,Owner")]
        public async Task<IActionResult> DeleteSchedulesByLoan(int loanId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            // Get the loan with member details for access validation
            var loan = await _context.Loans
                .Include(l => l.Member)
                .ThenInclude(m => m.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(l => l.Id == loanId && !l.IsDeleted);

            if (loan == null)
            {
                return NotFound("Loan not found");
            }

            // Check access based on user type
            if (userType == "Organization")
            {
                if (loan.Member.Center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || loan.Member.Center.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            var schedules = await _context.LoanSchedulers
                .Where(ls => ls.LoanId == loanId)
                .ToListAsync();

            if (!schedules.Any())
            {
                return NotFound("No schedules found for this loan");
            }

            // Check if any payments have been made
            var hasPaidSchedules = schedules.Any(s => s.Status == "Paid" || s.Status == "Partial");
            if (hasPaidSchedules)
            {
                return BadRequest("Cannot delete schedules with payments already made");
            }

            _context.LoanSchedulers.RemoveRange(schedules);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/LoanSchedulers/recovery
        // Returns one row per loan (next unpaid installment) for the Recovery Posting grid.
        [HttpGet("recovery")]
        public async Task<ActionResult<IEnumerable<MCS.WebApi.DTOs.LoanScheduler.LoanSchedulerRecoveryDto>>> GetLoanSchedulersForRecovery(
            [FromQuery] DateTime scheduleDate,
            [FromQuery] int? branchId,
            [FromQuery] int? centerId,
            [FromQuery] int? pocId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            // Enforce branch / org access if branch filter is specified
            if (branchId.HasValue)
            {
                if (userType == "Branch")
                {
                    if (!user.BranchId.HasValue || user.BranchId.Value != branchId.Value)
                    {
                        return Forbid();
                    }
                }
                else if (userType == "Organization")
                {
                    var branch = await _context.Branches.FindAsync(branchId.Value);
                    if (branch == null || branch.OrgId != user.OrgId)
                    {
                        return Forbid();
                    }
                }
            }

            var result = await _loanSchedulerService.GetLoanSchedulersForRecoveryAsync(
                scheduleDate,
                branchId,
                centerId,
                pocId,
                pageNumber,
                pageSize);

            Response.Headers["X-Total-Count"] = result.TotalCount.ToString();
            return Ok(result.Items);
        }

        // POST: api/LoanSchedulers/save
        // Single unified endpoint for single-row and bulk posting, using one transaction.
        [HttpPost("save")]
        [Authorize(Roles = "BranchAdmin,Staff,Owner")]
        public async Task<IActionResult> SaveLoanSchedulers([FromBody] List<MCS.WebApi.DTOs.LoanScheduler.LoanSchedulerSaveDto> items)
        {
            if (items == null || items.Count == 0)
            {
                return BadRequest("No installments provided.");
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            try
            {
                await _loanSchedulerService.SaveAsync(items, userId);
                return Ok(new { updatedCount = items.Count });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
