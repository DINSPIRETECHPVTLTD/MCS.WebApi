using MCS.WebApi.Data;
using MCS.WebApi.DTOs.Loan;
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
    public class LoansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly LoanSchedulerService _loanSchedulerService;
        private readonly LedgerTransactionService _ledgerTransactionService;
        private readonly ILogger<LoansController> _logger;

        public LoansController(
            ApplicationDbContext context, 
            LoanSchedulerService loanSchedulerService, 
            LedgerTransactionService ledgerTransactionService,
            ILogger<LoansController> logger)
        {
            _context = context;
            _loanSchedulerService = loanSchedulerService;
            _ledgerTransactionService = ledgerTransactionService;
            _logger = logger;
        }

        // POST: api/Loans
        [HttpPost]
        [Authorize(Roles = "BranchAdmin,Staff,Owner")]
        public async Task<ActionResult<Loan>> PostLoan(CreateLoanDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            // Validate that the member exists and user has access to it
            var member = await _context.Members
                .Include(m => m.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(m => m.Id == dto.MemberId);

            if (member == null)
            {
                return BadRequest("Member not found");
            }

            // Check access based on user type
            if (userType == "Organization")
            {
                if (member.Center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || member.Center.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            // Calculate total amount (if not provided)
            decimal totalAmount = dto.TotalAmount > 0 ? dto.TotalAmount : 
                dto.LoanAmount + dto.InterestAmount + dto.ProcessingFee + dto.InsuranceFee;

            var loan = new Loan
            {
                MemberId = dto.MemberId,
                LoanAmount = dto.LoanAmount,
                InterestAmount = dto.InterestAmount,
                ProcessingFee = dto.ProcessingFee,
                InsuranceFee = dto.InsuranceFee,
                IsSavingEnabled = dto.IsSavingEnabled,
                SavingAmount = dto.SavingAmount,
                TotalAmount = totalAmount,
                Status = "Active",
                DisbursementDate = dto.DisbursementDate ?? DateTime.UtcNow, 
                CollectionStartDate = dto.CollectionStartDate,
                CollectionTerm = dto.CollectionTerm,
                NoOfTerms = dto.NoOfTerms,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Automatically generate loan schedulers using the service
            if (loan.NoOfTerms > 0 && loan.CollectionStartDate.HasValue && !string.IsNullOrEmpty(loan.CollectionTerm))
            {
                try
                {
                    _logger.LogInformation($"Generating loan schedulers for loan {loan.Id}");
                    await _loanSchedulerService.GenerateEmiScheduleAsync(loan.Id, userId);
                    _logger.LogInformation($"Successfully generated loan schedulers for loan {loan.Id}");
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the loan creation
                    // The schedules can be generated manually later via the API endpoint
                    _logger.LogError(ex, $"Error generating loan schedulers for loan {loan.Id}: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning($"Loan {loan.Id} does not meet requirements for schedule generation. NoOfTerms: {loan.NoOfTerms}, CollectionStartDate: {loan.CollectionStartDate}, CollectionTerm: {loan.CollectionTerm}");
            }

            // Record ledger transactions for loan disbursement and fees
            try
            {
                _logger.LogInformation($"Recording ledger transactions for loan {loan.Id}");

                // 1. Record loan disbursement (money given to member)
                await _ledgerTransactionService.RecordDepositAsync(
                    paidToUserId: userId, // Assuming user is the one disbursing the loan
                    amount: loan.LoanAmount,
                    referenceId: loan.Id,
                    transactionType: "Loan disbursement",
                    comments: $"Loan disbursement for Loan ID: {loan.Id}, Member ID: {loan.MemberId}",
                    createdBy: userId
                );
                _logger.LogInformation($"Recorded loan disbursement of {loan.LoanAmount} for loan {loan.Id}");

                // 2. Record processing fee (collected from member)
                if (loan.ProcessingFee > 0)
                {
                    await _ledgerTransactionService.RecordDepositAsync(
                        paidToUserId: userId, // Organization receives the fee
                        amount: loan.ProcessingFee,
                        referenceId: loan.Id,
                        transactionType: "Processing fee",
                        comments: $"Processing fee for Loan ID: {loan.Id}, from Member ID: {loan.MemberId}",
                        createdBy: userId
                    );
                    _logger.LogInformation($"Recorded processing fee of {loan.ProcessingFee} for loan {loan.Id}");
                }

                // 3. Record insurance fee (collected from member)
                if (loan.InsuranceFee > 0)
                {
                    await _ledgerTransactionService.RecordDepositAsync(
                        paidToUserId: userId, // Organization receives the fee
                        amount: loan.InsuranceFee,
                        referenceId: loan.Id,
                        transactionType: "Insurance fee",
                        comments: $"Insurance fee for Loan ID: {loan.Id}, from Member ID: {loan.MemberId}",
                        createdBy: userId
                    );
                    _logger.LogInformation($"Recorded insurance fee of {loan.InsuranceFee} for loan {loan.Id}");
                }

                _logger.LogInformation($"Successfully recorded all ledger transactions for loan {loan.Id}");
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the loan creation
                // The transactions can be recorded manually later
                _logger.LogError(ex, $"Error recording ledger transactions for loan {loan.Id}: {ex.Message}");
            }

            return CreatedAtAction("GetLoan", new { id = loan.Id }, loan);
        }

        // GET: api/Loans/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Loan>> GetLoan(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            Loan? loan = null;

            if (userType == "Organization")
            {
                loan = await _context.Loans
                    .Include(l => l.Member)
                    .ThenInclude(m => m.Center)
                    .ThenInclude(c => c.Branch)
                    .FirstOrDefaultAsync(l => l.Id == id && l.Member.Center.Branch.OrgId == user.OrgId);
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                loan = await _context.Loans
                    .Include(l => l.Member)
                    .ThenInclude(m => m.Center)
                    .FirstOrDefaultAsync(l => l.Id == id && l.Member.Center.BranchId == user.BranchId.Value);
            }

            if (loan == null)
            {
                return NotFound();
            }

            return loan;
        }

        // GET: api/Loans
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoans()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            if (userType == "Organization")
            {
                return await _context.Loans
                    .Include(l => l.Member)
                    .ThenInclude(m => m.Center)
                    .ThenInclude(c => c.Branch)
                    .Where(l => l.Member.Center.Branch.OrgId == user.OrgId)
                    .ToListAsync();
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue)
                {
                    return Forbid();
                }
                return await _context.Loans
                    .Include(l => l.Member)
                    .ThenInclude(m => m.Center)
                    .Where(l => l.Member.Center.BranchId == user.BranchId.Value)
                    .ToListAsync();
            }

            return Forbid();
        }

        // GET: api/Loans/member/5
        [HttpGet("member/{memberId}")]
        public async Task<ActionResult<IEnumerable<Loan>>> GetLoansByMember(int memberId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            // Validate that the member exists and user has access to it
            var member = await _context.Members
                .Include(m => m.Center)
                .ThenInclude(c => c.Branch)
                .FirstOrDefaultAsync(m => m.Id == memberId);

            if (member == null)
            {
                return NotFound("Member not found");
            }

            // Check access based on user type
            if (userType == "Organization")
            {
                if (member.Center.Branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || member.Center.BranchId != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            var loans = await _context.Loans
                .Where(l => l.MemberId == memberId)
                .Include(l => l.LoanSchedulers)
                .ToListAsync();

            return Ok(loans);
        }

        // GET: api/Loans/branch/5
        [HttpGet("branch/{branchId}")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoansByBranch(int branchId)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userType = User.FindFirst("UserType")!.Value;
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Forbid();
            }

            // Validate that the branch exists and user has access to it
            var branch = await _context.Branches.FindAsync(branchId);

            if (branch == null)
            {
                return NotFound("Branch not found");
            }

            // Check access based on user type
            if (userType == "Organization")
            {
                if (branch.OrgId != user.OrgId)
                {
                    return Forbid();
                }
            }
            else if (userType == "Branch")
            {
                if (!user.BranchId.HasValue || branch.Id != user.BranchId.Value)
                {
                    return Forbid();
                }
            }

            var loans = await _context.Loans
                .Include(l => l.Member)
                .ThenInclude(m => m.Center)
                .ThenInclude(c => c.Branch)
                .Where(l => l.Member.Center.BranchId == branchId)
                .Include(l => l.LoanSchedulers)
                .ToListAsync();

                        var loanDtos = loans.Select(l => new LoanDto
                        {
                            Id = l.Id,
                            MemberId = l.MemberId,
                            LoanAmount = l.LoanAmount,
                            InterestAmount = l.InterestAmount,
                            ProcessingFee = l.ProcessingFee,
                            InsuranceFee = l.InsuranceFee,
                            IsSavingEnabled = l.IsSavingEnabled,
                            SavingAmount = l.SavingAmount,
                            TotalAmount = l.TotalAmount,
                            Status = l.Status,
                            DisbursementDate = l.DisbursementDate,
                            ClosureDate = l.ClosureDate,
                            CollectionStartDate = l.CollectionStartDate,
                            CollectionTerm = l.CollectionTerm,
                            NoOfTerms = l.NoOfTerms,
                            CreatedBy = l.CreatedBy,
                            CreatedAt = l.CreatedAt,
                            ModifiedBy = l.ModifiedBy,
                            ModifiedAt = l.ModifiedAt,
                            IsDeleted = l.IsDeleted
                        }).ToList();

                        return Ok(loanDtos);
                    }
                }
            }
