using MCS.WebApi.Data;
using MCS.WebApi.DTOs.LoanScheduler;
using MCS.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MCS.WebApi.Services
{
    public class LoanSchedulerService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoanSchedulerService> _logger;

        public LoanSchedulerService(ApplicationDbContext context, ILogger<LoanSchedulerService> logger)
        {
            _context = context;
            _logger = logger;
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
                    PaymentDate = null,
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

            // Adjust only Actual* fields for rounding drift.
            // Keep posted fields (Payment/Principal/Interest) as 0 for newly created schedules.
            if (schedules.Any())
            {
                var lastSchedule = schedules.Last();
                var totalActualPrincipal = schedules.Sum(s => s.ActualPrincipalAmount);
                var totalActualInterest = schedules.Sum(s => s.ActualInterestAmount);

                var principalDrift = loan.LoanAmount - totalActualPrincipal;
                var interestDrift = loan.InterestAmount - totalActualInterest;

                lastSchedule.ActualPrincipalAmount = Math.Round(lastSchedule.ActualPrincipalAmount + principalDrift, 2);
                lastSchedule.ActualInterestAmount = Math.Round(lastSchedule.ActualInterestAmount + interestDrift, 2);
                lastSchedule.ActualEmiAmount = Math.Round(lastSchedule.ActualPrincipalAmount + lastSchedule.ActualInterestAmount, 2);
            }

            var first = schedules.FirstOrDefault();
            var last = schedules.LastOrDefault();
            _logger.LogInformation(
                "Generated {Count} loan schedules for LoanId={LoanId}. First[Inst={FirstInst}, ActualEMI={FirstActualEmi}, Payment={FirstPayment}, Principal={FirstPrincipal}, Interest={FirstInterest}] Last[Inst={LastInst}, ActualEMI={LastActualEmi}, Payment={LastPayment}, Principal={LastPrincipal}, Interest={LastInterest}]",
                schedules.Count,
                loanId,
                first?.InstallmentNo,
                first?.ActualEmiAmount,
                first?.PaymentAmount,
                first?.PrincipalAmount,
                first?.InterestAmount,
                last?.InstallmentNo,
                last?.ActualEmiAmount,
                last?.PaymentAmount,
                last?.PrincipalAmount,
                last?.InterestAmount
            );

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
            int pageSize,
            string userType,
            int? orgId)
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
                    // Unpaid/not-yet-posted: null PaymentDate.
                    // Keep backward compatibility for older sentinel values already in DB.
                    (ls.PaymentDate == null || ls.PaymentDate.Value.Year == 9999))
                .ToListAsync();

            // Apply branch / center / POC filters in memory (after includes).
            // Tenant isolation: enforce org/branch scope regardless of provided query.
            if (string.Equals(userType, "Branch", StringComparison.OrdinalIgnoreCase))
            {
                if (!branchId.HasValue)
                {
                    return (Array.Empty<LoanSchedulerRecoveryDto>(), 0);
                }
                filteredList = filteredList
                    .Where(ls => ls.Loan.Member.Center.BranchId == branchId.Value)
                    .ToList();
            }
            else if (string.Equals(userType, "Organization", StringComparison.OrdinalIgnoreCase))
            {
                if (orgId.HasValue)
                {
                    filteredList = filteredList
                        .Where(ls => ls.Loan.Member.Center.Branch.OrgId == orgId.Value)
                        .ToList();
                }
            }
            else
            {
                return (Array.Empty<LoanSchedulerRecoveryDto>(), 0);
            }

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

                // Base schedule amounts for this installment:
                // - When PaymentAmount/PrincipalAmount/InterestAmount have been set/adjusted (e.g. via carry-forward), use them.
                // - Otherwise fall back to the original schedule stored in ActualEmiAmount/ActualPrincipalAmount/ActualInterestAmount.
                decimal baseTotal = ls.PaymentAmount > 0 ? ls.PaymentAmount : ls.ActualEmiAmount;
                decimal basePrincipal = ls.PaymentAmount > 0 ? ls.PrincipalAmount : ls.ActualPrincipalAmount;
                decimal baseInterest = ls.PaymentAmount > 0 ? ls.InterestAmount : ls.ActualInterestAmount;

                // Percentages for partial EMI split – MUST match how schedule was originally created.
                decimal principalPct = 0;
                decimal interestPct = 0;
                if (baseTotal > 0)
                {
                    principalPct = Math.Round((basePrincipal / baseTotal) * 100, 2);
                    interestPct = Math.Round((baseInterest / baseTotal) * 100, 2);
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
                    // Use base* so grid sees the same split that was used when creating the loan / adjusting EMIs.
                    InterestAmount = baseInterest,
                    PrincipalAmount = basePrincipal,
                    PaymentAmount = baseTotal,
                    Status = ls.Status,
                    Due = baseTotal,
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
        public async Task SaveAsync(
            IEnumerable<LoanSchedulerSaveDto> items,
            int currentUserId,
            string userType,
            int? orgId,
            int? branchId)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            var list = items.ToList();
            if (list.Count == 0)
            {
                throw new InvalidOperationException("No installments provided to save.");
            }

            // Request-level safety: reject duplicate scheduler ids in same bulk post.
            var distinctCount = list.Select(x => x.LoanSchedulerId).Distinct().Count();
            if (distinctCount != list.Count)
            {
                throw new InvalidOperationException("Duplicate LoanSchedulerId found in request payload.");
            }

            // Load valid payment modes once per request (fintech contract integrity).
            // Backward-compatible: accept either LookupValue or LookupCode. If there are no PAYMENTMODE rows configured, we won't block.
            var paymentModeLookups = await _context.MasterLookups
                .Where(m => m.LookupKey == LookupKeys.PaymentMode && m.IsActive)
                .Select(m => new { m.LookupCode, m.LookupValue })
                .ToListAsync();

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Use single timestamp for entire post request so all rows get same PaymentDate
                var postRequestDateTime = DateTime.UtcNow;

                foreach (var dto in list)
                {
                    var schedule = await _context.LoanSchedulers
                        .Include(ls => ls.Loan)
                            .ThenInclude(l => l.Member)
                                .ThenInclude(m => m.Center)
                                    .ThenInclude(c => c.Branch)
                        .FirstOrDefaultAsync(ls => ls.LoanSchedulerId == dto.LoanSchedulerId);

                    if (schedule == null)
                    {
                        throw new InvalidOperationException($"Schedule {dto.LoanSchedulerId} not found.");
                    }

                    // Replay protection: reject any already-posted schedule (even if status isn't Paid).
                    if (schedule.PaymentDate.HasValue && schedule.PaymentDate.Value.Year != 9999)
                    {
                        throw new InvalidOperationException($"Schedule {schedule.LoanSchedulerId} is already posted.");
                    }

                    // Prevent updating only when DB status is already Paid.
                    if (string.Equals(schedule.Status?.Trim(), "Paid", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException($"Schedule {schedule.LoanSchedulerId} is already paid or posted.");
                    }

                    // Round monetary values to 2 decimals BEFORE any validation.
                    var paymentAmount = Math.Round(dto.PaymentAmount, 2, MidpointRounding.AwayFromZero);
                    if (paymentAmount < 0)
                    {
                        throw new InvalidOperationException("Amounts cannot be negative.");
                    }

                    // Authorization: enforce resource-level access for each schedule being posted.
                    // Branch user can only post within their branch. Organization user can only post within their organization.
                    if (string.Equals(userType, "Branch", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!branchId.HasValue || schedule.Loan.Member.Center.BranchId != branchId.Value)
                        {
                            throw new InvalidOperationException("Unauthorized: cannot post installments outside your branch.");
                        }
                    }
                    else if (string.Equals(userType, "Organization", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!orgId.HasValue || schedule.Loan.Member.Center.Branch.OrgId != orgId.Value)
                        {
                            throw new InvalidOperationException("Unauthorized: cannot post installments outside your organization.");
                        }
                    }

                    var scheduledAmount = Math.Round(schedule.ActualEmiAmount, 2, MidpointRounding.AwayFromZero);

                    // Normalize status in a non-ambiguous way.
                    // Backward-compatible: if status is omitted, derive from payment vs scheduled amounts.
                    var requestedStatus = NormalizeOrDeriveRequestedStatus(dto.Status, paymentAmount, scheduledAmount);

                    // Contract: For Paid/Partial, payment must be strictly > 0.
                    if (requestedStatus != "Not Paid" && paymentAmount <= 0)
                    {
                        throw new InvalidOperationException("For Paid/Partial Paid, PaymentAmount must be greater than 0.");
                    }

                    // DO NOT trust client principal/interest split.
                    // Compute split server-side using schedule ratio, then enforce invariants on computed values.
                    decimal principalRatio = 0m;
                    if (scheduledAmount > 0 && schedule.ActualPrincipalAmount > 0)
                    {
                        principalRatio = schedule.ActualPrincipalAmount / scheduledAmount;
                    }
                    var principalAmount = Math.Round(paymentAmount * principalRatio, 2, MidpointRounding.AwayFromZero);
                    var interestAmount = paymentAmount - principalAmount; // stays 2 decimals since both are 2 decimals

                    // Fintech invariant: Principal + Interest must equal PaymentAmount (after rounding).
                    if (Math.Abs((principalAmount + interestAmount) - paymentAmount) > 0.00m)
                    {
                        throw new InvalidOperationException("PrincipalAmount + InterestAmount must equal PaymentAmount.");
                    }

                    var paymentMode = (dto.PaymentMode ?? string.Empty).Trim();
                    if (requestedStatus == "Not Paid")
                    {
                        if (paymentAmount != 0)
                        {
                            throw new InvalidOperationException("For Status Not Paid, Payment/Principal/Interest amounts must be 0.");
                        }

                        if (string.IsNullOrWhiteSpace(dto.Comments))
                        {
                            throw new InvalidOperationException("Comment is required for Not Paid.");
                        }

                        paymentMode = "N/A";
                        principalAmount = 0;
                        interestAmount = 0;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(paymentMode))
                        {
                            throw new InvalidOperationException("Payment mode is required for Paid/Partial Paid.");
                        }

                        if (string.Equals(paymentMode, "N/A", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException("Payment mode cannot be N/A for Paid/Partial Paid.");
                        }

                        // Normalize PaymentMode against MasterLookups if configured.
                        if (paymentModeLookups.Count > 0)
                        {
                            var match = paymentModeLookups.FirstOrDefault(m =>
                                string.Equals(m.LookupValue, paymentMode, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(m.LookupCode, paymentMode, StringComparison.OrdinalIgnoreCase));

                            if (match == null)
                            {
                                throw new InvalidOperationException("Invalid payment mode.");
                            }

                            // Store lookup VALUE consistently (frontend displays value).
                            paymentMode = match.LookupValue;
                        }
                    }

                    if (paymentAmount > scheduledAmount)
                    {
                        throw new InvalidOperationException("Payment amount cannot exceed scheduled amount.");
                    }

                    // If user selected Status = Paid, payment amount must match scheduled amount
                    if (string.Equals((dto.Status ?? string.Empty).Trim(), "Paid", StringComparison.OrdinalIgnoreCase) &&
                        Math.Abs(paymentAmount - scheduledAmount) > 0.01m)
                    {
                        throw new InvalidOperationException(
                            "For Status Paid, payment amount must match scheduled amount. Change status to Partial Paid.");
                    }

                    // Post only: PaymentDate (current post request date/time), PaymentAmount, PrincipalAmount, InterestAmount, PaymentMode, Status, Comments, CollectedBy. Do NOT update Actual*, LoanId, LoanSchedulerId, InstallmentNo, ScheduleDate, CreatedBy, CreatedDate.
                    schedule.PaymentDate = postRequestDateTime;
                    schedule.PaymentAmount = paymentAmount;
                    schedule.PrincipalAmount = principalAmount;
                    schedule.InterestAmount = interestAmount;
                    schedule.PaymentMode = paymentMode;
                    schedule.Comments = (dto.Comments ?? string.Empty).Trim();
                    schedule.CollectedBy = dto.CollectedBy ?? currentUserId;

                    // Difference amounts between scheduled (Actual*) and posted amounts.
                    // Null-safe by design (all decimal non-nullable on entity/DTO), and clamped to 0.
                    decimal differenceAmount = Math.Max(0, schedule.ActualEmiAmount - paymentAmount);
                    decimal differencePrincipalAmount = Math.Max(0, schedule.ActualPrincipalAmount - principalAmount);
                    decimal differenceInterestAmount = Math.Max(0, schedule.ActualInterestAmount - interestAmount);

                    // Carry-forward to immediate next unpaid installment.
                    if (requestedStatus == "Not Paid")
                    {
                        schedule.Status = "Not Paid";

                        var nextSchedule = await _context.LoanSchedulers
                            .Where(ls =>
                                ls.LoanId == schedule.LoanId &&
                                ls.InstallmentNo > schedule.InstallmentNo &&
                                ls.Status != "Paid" &&
                                (ls.PaymentDate == null || ls.PaymentDate.Value.Year == 9999))
                            .OrderBy(ls => ls.InstallmentNo)
                            .FirstOrDefaultAsync();

                        if (nextSchedule == null)
                        {
                            throw new InvalidOperationException(
                                $"No next installment found to carry forward remaining amount for loan {schedule.LoanId}.");
                        }

                        nextSchedule.ActualEmiAmount = Math.Round(nextSchedule.ActualEmiAmount + differenceAmount, 2);
                        nextSchedule.ActualInterestAmount = Math.Round(nextSchedule.ActualInterestAmount + differenceInterestAmount, 2);
                        nextSchedule.ActualPrincipalAmount = Math.Round(nextSchedule.ActualPrincipalAmount + differencePrincipalAmount, 2);
                    }
                    else if (differenceAmount == 0)
                    {
                        schedule.Status = "Paid";
                    }
                    else
                    {
                        schedule.Status = "Partial";

                        // Carry forward the remaining amount to next unpaid installment for this loan.
                        var nextSchedule = await _context.LoanSchedulers
                            .Where(ls =>
                                ls.LoanId == schedule.LoanId &&
                                ls.InstallmentNo > schedule.InstallmentNo &&
                                ls.Status != "Paid" &&
                                (ls.PaymentDate == null || ls.PaymentDate.Value.Year == 9999))
                            .OrderBy(ls => ls.InstallmentNo)
                            .FirstOrDefaultAsync();

                        if (nextSchedule == null)
                        {
                            throw new InvalidOperationException(
                                $"No next installment found to carry forward remaining amount for loan {schedule.LoanId}.");
                        }

                        // Add Difference* to next EMI Actual* columns (business logic unchanged, column names updated).
                        // 1) DifferenceAmount -> next ActualEmiAmount
                        nextSchedule.ActualEmiAmount = Math.Round(nextSchedule.ActualEmiAmount + differenceAmount, 2);
                        // 2) DifferenceInterestAmount -> next ActualInterestAmount
                        nextSchedule.ActualInterestAmount = Math.Round(nextSchedule.ActualInterestAmount + differenceInterestAmount, 2);
                        // 3) DifferencePrincipalAmount -> next ActualPrincipalAmount
                        nextSchedule.ActualPrincipalAmount = Math.Round(nextSchedule.ActualPrincipalAmount + differencePrincipalAmount, 2);
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

        private static string NormalizeOrDeriveRequestedStatus(string? status, decimal paymentAmount, decimal scheduledAmount)
        {
            var normalized = (status ?? string.Empty).Trim().ToLowerInvariant();
            if (normalized == "paid") return "Paid";
            if (normalized == "partial" || normalized == "partial paid" || normalized == "partialpaid") return "Partial";
            if (normalized == "not paid" || normalized == "notpaid") return "Not Paid";

            // Derive if not provided / unknown: never silently guess Partial without evidence.
            if (paymentAmount <= 0) return "Not Paid";
            return Math.Abs(paymentAmount - scheduledAmount) <= 0.01m ? "Paid" : "Partial";
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
