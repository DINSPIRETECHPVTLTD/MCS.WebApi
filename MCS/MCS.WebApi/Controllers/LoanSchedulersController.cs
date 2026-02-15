using MCS.WebApi.Data;
using MCS.WebApi.Models;
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

        public LoanSchedulersController(ApplicationDbContext context)
        {
            _context = context;
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

                // Validate loan has required data
                if (loan.NoOfTerms <= 0)
                {
                    return BadRequest("Invalid number of terms");
                }

                if (loan.CollectionStartDate == null)
                {
                    return BadRequest("Collection start date is required");
                }

                if (string.IsNullOrEmpty(loan.CollectionTerm))
                {
                    return BadRequest("Collection term is required");
                }

                // Calculate payment amounts
                decimal totalLoanAmount = loan.LoanAmount + loan.InterestAmount;
                decimal principalPerInstallment = loan.LoanAmount / loan.NoOfTerms;
                decimal interestPerInstallment = loan.InterestAmount / loan.NoOfTerms;
                decimal paymentPerInstallment = totalLoanAmount / loan.NoOfTerms;

                var schedules = new List<LoanScheduler>();
                DateTime currentDate = loan.CollectionStartDate.Value;

                for (int i = 1; i <= loan.NoOfTerms; i++)
                {
                    var schedule = new LoanScheduler
                    {
                        LoanId = loanId,
                        ScheduleDate = currentDate,
                        PaymentDate = currentDate,
                        PaymentAmount = Math.Round(paymentPerInstallment, 2),
                        SavingAmount = loan.IsSavingEnabled ? Math.Round(loan.SavingAmount / loan.NoOfTerms, 2) : 0,
                        PrincipalAmount = Math.Round(principalPerInstallment, 2),
                        InterestAmount = Math.Round(interestPerInstallment, 2),
                        CollectedBy = userId,
                        InstallmentNo = i,
                        Status = "Not Paid",
                        CreatedBy = userId,
                        CreatedDate = DateTime.UtcNow
                    };

                    schedules.Add(schedule);

                    // Calculate next payment date based on collection term
                    currentDate = CalculateNextPaymentDate(currentDate, loan.CollectionTerm);
                }

                // Adjust last installment to account for rounding differences
                if (schedules.Any())
                {
                    var lastSchedule = schedules.Last();
                    var totalScheduledPrincipal = schedules.Sum(s => s.PrincipalAmount);
                    var totalScheduledInterest = schedules.Sum(s => s.InterestAmount);

                    lastSchedule.PrincipalAmount += loan.LoanAmount - totalScheduledPrincipal;
                    lastSchedule.InterestAmount += loan.InterestAmount - totalScheduledInterest;
                    lastSchedule.PaymentAmount = lastSchedule.PrincipalAmount + lastSchedule.InterestAmount;
                }

                _context.LoanSchedulers.AddRange(schedules);
                await _context.SaveChangesAsync();

                return Ok(schedules);
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

                    // Helper method to calculate next payment date
                    private DateTime CalculateNextPaymentDate(DateTime currentDate, string collectionTerm)
                    {
                        return collectionTerm.ToLower() switch
                        {
                            "daily" => currentDate.AddDays(1),
                            "weekly" => currentDate.AddDays(7),
                            "biweekly" or "bi-weekly" => currentDate.AddDays(14),
                            "monthly" => currentDate.AddMonths(1),
                            "quarterly" => currentDate.AddMonths(3),
                            "half-yearly" or "semi-annual" => currentDate.AddMonths(6),
                            "yearly" or "annual" => currentDate.AddYears(1),
                            _ => currentDate.AddDays(7) // Default to weekly
                        };
                    }
                }
            }
