using MCS.WebApi.Data;
using MCS.WebApi.DTOs.LoanScheduler;
using MCS.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MCS.WebApi.Services
{
    public class LoanSchedulerService
    {
        private readonly ApplicationDbContext _context;

        public LoanSchedulerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<LoanScheduler>> GenerateEmiScheduleAsync(int loanId, int userId)
        {
            // Get the loan
            var loan = await _context.Loans.FindAsync(loanId);
            if (loan == null)
            {
                throw new InvalidOperationException("Loan not found");
            }

            // Check if schedules already exist
            var existingSchedules = await _context.LoanSchedulers
                .Where(ls => ls.LoanId == loanId)
                .AnyAsync();

            if (existingSchedules)
            {
                throw new InvalidOperationException("EMI schedule already exists for this loan");
            }

            // Validate loan data
            if (loan.NoOfTerms <= 0)
            {
                throw new InvalidOperationException("Invalid number of terms");
            }

            if (loan.CollectionStartDate == null)
            {
                throw new InvalidOperationException("Collection start date is required");
            }

            if (string.IsNullOrEmpty(loan.CollectionTerm))
            {
                throw new InvalidOperationException("Collection term is required");
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
                    PaymentDate = new DateTime(9999, 1, 1),
                    PaymentAmount = 0,
                    SavingAmount = loan.IsSavingEnabled ? Math.Round(loan.SavingAmount / loan.NoOfTerms, 2) : 0,
                    PrincipalAmount = 0,
                    InterestAmount = 0,
                    ActualEmiAmount = Math.Round(paymentPerInstallment, 2),
                    ActualPrincipalAmount = Math.Round(principalPerInstallment, 2),
                    ActualInterestAmount = Math.Round(interestPerInstallment, 2),
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

            // Save to database
            _context.LoanSchedulers.AddRange(schedules);
            await _context.SaveChangesAsync();

            return schedules;
        }

        /// <summary>
        /// Returns one row per loan: the next unpaid installment for the given schedule date and optional Center/POC filters,
        /// shaped for the Recovery Posting grid.
        /// </summary>
        public async Task<(IReadOnlyList<LoanSchedulerRecoveryDto> Items, int TotalCount)> GetLoanSchedulersForRecoveryAsync(
            DateTime scheduleDate,
            int? branchId,
            int? centerId,
            int? pocId,
            int pageNumber,
            int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 50;

            // Base query: schedules for the given date, not yet paid/posted.
            // We intentionally materialize to memory before grouping, to avoid complex
            // GroupBy + navigation translation issues in EF Core.
            var startDate = scheduleDate.Date;
            var endDate = startDate.AddDays(1);

            var filteredList = await _context.LoanSchedulers
                .Include(ls => ls.Loan)
                    .ThenInclude(l => l.Member)
                        .ThenInclude(m => m.Center)
                            .ThenInclude(c => c.Branch)
                .Include(ls => ls.Loan)
                    .ThenInclude(l => l.Member)
                        .ThenInclude(m => m.POC)
                .Where(ls =>
                    ls.ScheduleDate >= startDate &&
                    ls.ScheduleDate < endDate &&
                    ls.Status != "Paid" &&
                    ls.PaymentDate == default) // default(DateTime) == not yet posted
                .ToListAsync();

            // Apply branch / center / POC filters in memory (after includes).
            if (branchId.HasValue)
            {
                filteredList = filteredList
                    .Where(ls => ls.Loan.Member.Center.BranchId == branchId.Value)
                    .ToList();
            }

            if (centerId.HasValue)
            {
                filteredList = filteredList
                    .Where(ls => ls.Loan.Member.CenterId == centerId.Value)
                    .ToList();
            }

            if (pocId.HasValue)
            {
                filteredList = filteredList
                    .Where(ls => ls.Loan.Member.POCId == pocId.Value)
                    .ToList();
            }

            // Only next unpaid installment per loan (grouping done in memory).
            var nextPerLoan = filteredList
                .GroupBy(ls => ls.LoanId)
                .Select(g => g.OrderBy(x => x.InstallmentNo).First())
                .ToList();

            var totalCount = nextPerLoan.Count;

            var page = nextPerLoan
                .OrderBy(ls => ls.ScheduleDate)
                .ThenBy(ls => ls.LoanId)
                .ThenBy(ls => ls.InstallmentNo)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = page.Select(ls =>
            {
                var member = ls.Loan.Member;
                var center = member.Center;
                var poc = member.POC;

                string memberName = string.Join(" ",
                    new[]
                    {
                        member.FirstName,
                        member.MiddleName,
                        member.LastName
                    }.Where(s => !string.IsNullOrWhiteSpace(s)));

                string pocName = string.Join(" ",
                    new[]
                    {
                        poc.FirstName,
                        poc.MiddleName,
                        poc.LastName
                    }.Where(s => !string.IsNullOrWhiteSpace(s)));

                // Actual amounts are non-nullable on entity; treat 0 as "not yet filled" for UI.
                decimal? actualEmi = ls.ActualEmiAmount > 0 ? ls.ActualEmiAmount : (decimal?)null;
                decimal? actualInterest = ls.ActualInterestAmount > 0 ? ls.ActualInterestAmount : (decimal?)null;
                decimal? actualPrincipal = ls.ActualPrincipalAmount > 0 ? ls.ActualPrincipalAmount : (decimal?)null;

                // Percentages for partial EMI split (from this installment's scheduled split).
                decimal principalPct = 0;
                decimal interestPct = 0;
                if (ls.PaymentAmount > 0)
                {
                    principalPct = Math.Round((ls.PrincipalAmount / ls.PaymentAmount) * 100, 2);
                    interestPct = Math.Round((ls.InterestAmount / ls.PaymentAmount) * 100, 2);
                }

                return new LoanSchedulerRecoveryDto
                {
                    LoanSchedulerId = ls.LoanSchedulerId,
                    LoanId = ls.LoanId,
                    MemberId = member.Id,
                    MemberName = memberName,
                    CenterName = center.Name,
                    ParentPocName = pocName,
                    ScheduleDate = ls.ScheduleDate,
                    InstallmentNo = ls.InstallmentNo,
                    InterestAmount = ls.InterestAmount,
                    PrincipalAmount = ls.PrincipalAmount,
                    PaymentAmount = ls.PaymentAmount,
                    Status = ls.Status,
                    Due = ls.PaymentAmount,
                    ActualEmiAmount = actualEmi,
                    ActualInterestAmount = actualInterest,
                    ActualPrincipalAmount = actualPrincipal,
                    Comments = ls.Comments,
                    PrincipalPercentage = principalPct,
                    InterestPercentage = interestPct
                };
            }).ToList();

            return (items, totalCount);
        }

        /// <summary>
        /// Saves one or more loan scheduler installments (single or bulk) in a single database transaction,
        /// applying partial-paid carry-forward rules.
        /// </summary>
        public async Task SaveAsync(IEnumerable<LoanSchedulerSaveDto> items, int currentUserId)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var list = items.ToList();
            if (list.Count == 0)
            {
                throw new InvalidOperationException("No installments provided to save.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var dto in list)
                {
                    var schedule = await _context.LoanSchedulers
                        .Include(ls => ls.Loan)
                            .ThenInclude(l => l.Member)
                                .ThenInclude(m => m.Center)
                        .FirstOrDefaultAsync(ls => ls.LoanSchedulerId == dto.LoanSchedulerId);

                    if (schedule == null)
                    {
                        throw new InvalidOperationException($"Schedule {dto.LoanSchedulerId} not found.");
                    }

                    // Prevent updating already paid/posted records.
                    if (schedule.Status == "Paid" || schedule.PaymentDate != default)
                    {
                        throw new InvalidOperationException($"Schedule {schedule.LoanSchedulerId} is already paid or posted.");
                    }

                    if (dto.ActualEmiAmount < 0 ||
                        dto.ActualInterestAmount < 0 ||
                        dto.ActualPrincipalAmount < 0)
                    {
                        throw new InvalidOperationException("Amounts cannot be negative.");
                    }

                    if (dto.ActualEmiAmount > schedule.PaymentAmount)
                    {
                        throw new InvalidOperationException("Actual EMI amount cannot exceed scheduled payment amount.");
                    }

                    // If user selected Status = Paid, Actual Paid Amount must match Payment Amount
                    if (string.Equals(dto.Status, "Paid", StringComparison.OrdinalIgnoreCase) &&
                        Math.Abs(dto.ActualEmiAmount - schedule.PaymentAmount) > 0.01m)
                    {
                        throw new InvalidOperationException(
                            "Actual Paid Amount does not match Payment Amount. Change status to Partial Paid.");
                    }

                    // Comment is required when Status is Partial Paid; not required when Paid
                    var isPartialPaid = string.Equals(dto.Status, "Partial", StringComparison.OrdinalIgnoreCase);
                    if (isPartialPaid && string.IsNullOrWhiteSpace(dto.Comments))
                    {
                        throw new InvalidOperationException("Comment is required.");
                    }

                    // Re-calculate actual principal and interest from ActualEmiAmount and schedule ratio (so they sum exactly to ActualEmiAmount).
                    decimal recalcActualPrincipal = schedule.PaymentAmount > 0
                        ? Math.Round(dto.ActualEmiAmount * (schedule.PrincipalAmount / schedule.PaymentAmount), 2)
                        : 0;
                    decimal recalcActualInterest = Math.Round(dto.ActualEmiAmount - recalcActualPrincipal, 2);

                    // Update main schedule fields.
                    schedule.PaymentMode = dto.PaymentMode;
                    schedule.Comments = dto.Comments;
                    schedule.CollectedBy = dto.CollectedBy ?? currentUserId;
                    schedule.ActualEmiAmount = dto.ActualEmiAmount;
                    schedule.ActualInterestAmount = recalcActualInterest;
                    schedule.ActualPrincipalAmount = recalcActualPrincipal;
                    schedule.PaymentDate = DateTime.UtcNow;

                    // Determine new status and carry-forward.
                    decimal difference = schedule.PaymentAmount - dto.ActualEmiAmount;
                    if (difference == 0)
                    {
                        schedule.Status = "Paid";
                    }
                    else if (difference > 0)
                    {
                        schedule.Status = "Partial";

                        // Carry forward the remaining amount to next unpaid installment for this loan.
                        var nextSchedule = await _context.LoanSchedulers
                            .Where(ls =>
                                ls.LoanId == schedule.LoanId &&
                                ls.InstallmentNo > schedule.InstallmentNo &&
                                ls.Status != "Paid" &&
                                ls.PaymentDate == default)
                            .OrderBy(ls => ls.InstallmentNo)
                            .FirstOrDefaultAsync();

                        if (nextSchedule == null)
                        {
                            throw new InvalidOperationException(
                                $"No next installment found to carry forward remaining amount for loan {schedule.LoanId}.");
                        }

                        // Add difference to next EMI and recalculate PrincipalAmount and InterestAmount by same ratio
                        decimal newPaymentAmount = nextSchedule.PaymentAmount + difference;
                        decimal nextPrincipalPct = nextSchedule.PaymentAmount > 0
                            ? (nextSchedule.PrincipalAmount / nextSchedule.PaymentAmount) * 100
                            : 0;
                        decimal nextInterestPct = nextSchedule.PaymentAmount > 0
                            ? (nextSchedule.InterestAmount / nextSchedule.PaymentAmount) * 100
                            : 0;
                        nextSchedule.PaymentAmount = Math.Round(newPaymentAmount, 2);
                        nextSchedule.PrincipalAmount = Math.Round((newPaymentAmount * nextPrincipalPct) / 100, 2);
                        nextSchedule.InterestAmount = Math.Round(newPaymentAmount - nextSchedule.PrincipalAmount, 2);
                    }
                    else
                    {
                        // Should not happen because of earlier validation.
                        throw new InvalidOperationException("Actual EMI amount is greater than scheduled payment.");
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

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
