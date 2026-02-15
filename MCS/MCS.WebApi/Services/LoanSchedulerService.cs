using MCS.WebApi.Data;
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
                    PaymentAmount = Math.Round(paymentPerInstallment, 2),
                    SavingAmount = loan.IsSavingEnabled ? Math.Round(loan.SavingAmount / loan.NoOfTerms, 2) : 0,
                    PrincipalAmount = Math.Round(principalPerInstallment, 2),
                    InterestAmount = Math.Round(interestPerInstallment, 2),
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
