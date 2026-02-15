using MCS.WebApi.Data;
using MCS.WebApi.DTOs.Loan;
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
    public class LoansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoansController> _logger;

        public LoansController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, ILogger<LoansController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
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

            // Automatically generate loan schedulers by calling the LoanSchedulers API
            if (loan.NoOfTerms > 0 && loan.CollectionStartDate.HasValue && !string.IsNullOrEmpty(loan.CollectionTerm))
            {
                try
                {
                    var httpClient = _httpClientFactory.CreateClient();
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var generateUrl = $"{baseUrl}/api/LoanSchedulers/generate/{loan.Id}";

                    _logger.LogInformation($"Attempting to generate loan schedulers for loan {loan.Id} at {generateUrl}");

                    // Copy the authorization token from the current request
                    if (Request.Headers.TryGetValue("Authorization", out var authHeader))
                    {
                        httpClient.DefaultRequestHeaders.Add("Authorization", authHeader.ToString());
                        _logger.LogInformation("Authorization header added to request");
                    }
                    else
                    {
                        _logger.LogWarning("No Authorization header found in request");
                    }

                    var response = await httpClient.PostAsync(generateUrl, null);

                    if (!response.IsSuccessStatusCode)
                    {
                        // Log the error but don't fail the loan creation
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError($"Failed to generate loan schedulers for loan {loan.Id}. Status: {response.StatusCode}, Error: {errorContent}");
                    }
                    else
                    {
                        _logger.LogInformation($"Successfully generated loan schedulers for loan {loan.Id}");
                    }
                }
                catch (Exception ex)
                {
                    // Log the error but don't fail the loan creation
                    _logger.LogError(ex, $"Error calling LoanSchedulers API for loan {loan.Id}: {ex.Message}");
                }
            }
            else
            {
                _logger.LogWarning($"Loan {loan.Id} does not meet requirements for schedule generation. NoOfTerms: {loan.NoOfTerms}, CollectionStartDate: {loan.CollectionStartDate}, CollectionTerm: {loan.CollectionTerm}");
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
