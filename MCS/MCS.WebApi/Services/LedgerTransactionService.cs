using MCS.WebApi.Data;
using MCS.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace MCS.WebApi.Services
{
    public class LedgerTransactionService
    {
        private readonly ApplicationDbContext _context;

        public LedgerTransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a ledger transaction and updates the ledger balances for both parties
        /// </summary>
        /// <param name="paidFromUserId">User ID who is paying (optional for cash deposits)</param>
        /// <param name="paidToUserId">User ID who is receiving (optional for cash withdrawals)</param>
        /// <param name="amount">Transaction amount</param>
        /// <param name="transactionType">Type of transaction (e.g., "Payment", "Deposit", "Withdrawal", "Transfer")</param>
        /// <param name="referenceId">Optional reference ID (e.g., LoanId, MembershipFeeId)</param>
        /// <param name="comments">Optional comments</param>
        /// <param name="createdBy">User ID who created the transaction</param>
        /// <returns>Created LedgerTransaction</returns>
        public async Task<LedgerTransaction> CreateTransactionAsync(
            int? paidFromUserId,
            int? paidToUserId,
            decimal amount,
            string transactionType,
            int? referenceId = null,
            string? comments = null,
            int? createdBy = null)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("Transaction amount must be greater than zero");
            }

            if (paidFromUserId == null && paidToUserId == null)
            {
                throw new InvalidOperationException("Either PaidFromUserId or PaidToUserId must be specified");
            }

            // Validate users exist
            if (paidFromUserId.HasValue)
            {
                var fromUserExists = await _context.Users.AnyAsync(u => u.Id == paidFromUserId.Value);
                if (!fromUserExists)
                {
                    throw new InvalidOperationException($"User with ID {paidFromUserId} not found");
                }
            }

            if (paidToUserId.HasValue)
            {
                var toUserExists = await _context.Users.AnyAsync(u => u.Id == paidToUserId.Value);
                if (!toUserExists)
                {
                    throw new InvalidOperationException($"User with ID {paidToUserId} not found");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Create the ledger transaction
                var ledgerTransaction = new LedgerTransaction
                {
                    PaidFromUserId = paidFromUserId,
                    PaidToUserId = paidToUserId,
                    Amount = amount,
                    PaymentDate = DateTime.UtcNow,
                    TransactionType = transactionType,
                    ReferenceId = referenceId,
                    Comments = comments,
                    CreatedBy = createdBy ?? paidFromUserId ?? paidToUserId ?? 0,
                    CreatedDate = DateTime.UtcNow
                };

                _context.LedgerTransactions.Add(ledgerTransaction);

                // Update ledger balances
                if (paidFromUserId.HasValue)
                {
                    await UpdateLedgerBalanceAsync(paidFromUserId.Value, -amount); // Debit
                }

                if (paidToUserId.HasValue)
                {
                    await UpdateLedgerBalanceAsync(paidToUserId.Value, amount); // Credit
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ledgerTransaction;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Records a payment from one user to another
        /// </summary>
        public async Task<LedgerTransaction> RecordPaymentAsync(
            int paidFromUserId,
            int paidToUserId,
            decimal amount,
            string transactionType,
            int? referenceId = null,
            string? comments = null,
            int? createdBy = null)
        {
            return await CreateTransactionAsync(
                paidFromUserId: paidFromUserId,
                paidToUserId: paidToUserId,
                amount: amount,
                transactionType: transactionType,
                referenceId: referenceId,
                comments: comments,
                createdBy: createdBy
            );
        }

        /// <summary>
        /// Records a cash deposit to a user's account
        /// </summary>
        public async Task<LedgerTransaction> RecordDepositAsync(
            int paidToUserId,
            decimal amount,
            string transactionType = "Deposit",
            int? referenceId = null,
            string? comments = null,
            int? createdBy = null)
        {
            return await CreateTransactionAsync(
                paidFromUserId: null, // No from user for deposits
                paidToUserId: paidToUserId,
                amount: amount,
                transactionType: transactionType,
                referenceId: referenceId,
                comments: comments,
                createdBy: createdBy
            );
        }

        /// <summary>
        /// Records a cash withdrawal from a user's account
        /// </summary>
        public async Task<LedgerTransaction> RecordWithdrawalAsync(
            int paidFromUserId,
            decimal amount,
            string transactionType,
            int? referenceId = null,
            string? comments = null,
            int? createdBy = null)
        {
            // Check if user has sufficient balance
            var ledger = await GetOrCreateLedgerAsync(paidFromUserId);
            if (ledger.Amount < amount)
            {
                throw new InvalidOperationException($"Insufficient balance. Available: {ledger.Amount}, Required: {amount}");
            }

            return await CreateTransactionAsync(
                paidFromUserId: paidFromUserId,
                paidToUserId: null, // No to user for withdrawals
                amount: amount,
                transactionType: transactionType,
                referenceId: referenceId,
                comments: comments,
                createdBy: createdBy
            );
        }

        /// <summary>
        /// Records a transfer between two users
        /// </summary>
        public async Task<LedgerTransaction> RecordTransferAsync(
            int paidFromUserId,
            int paidToUserId,
            decimal amount,
            int? referenceId = null,
            string? comments = null,
            int? createdBy = null)
        {
            // Check if from user has sufficient balance
            var ledger = await GetOrCreateLedgerAsync(paidFromUserId);
            if (ledger.Amount < amount)
            {
                throw new InvalidOperationException($"Insufficient balance. Available: {ledger.Amount}, Required: {amount}");
            }

            return await CreateTransactionAsync(
                paidFromUserId: paidFromUserId,
                paidToUserId: paidToUserId,
                amount: amount,
                transactionType: "Transfer",
                referenceId: referenceId,
                comments: comments,
                createdBy: createdBy
            );
        }

        /// <summary>
        /// Gets the current ledger balance for a user
        /// </summary>
        public async Task<decimal> GetBalanceAsync(int userId)
        {
            var ledger = await _context.Ledgers
                .FirstOrDefaultAsync(l => l.UserId == userId);

            return ledger?.Amount ?? 0;
        }

        /// <summary>
        /// Gets all transactions for a user
        /// </summary>
        public async Task<List<LedgerTransaction>> GetUserTransactionsAsync(int userId)
        {
            return await _context.LedgerTransactions
                .Where(lt => lt.PaidFromUserId == userId || lt.PaidToUserId == userId)
                .OrderByDescending(lt => lt.PaymentDate)
                .ToListAsync();
        }

        /// <summary>
        /// Updates the ledger balance for a user
        /// </summary>
        private async Task UpdateLedgerBalanceAsync(int userId, decimal amountChange)
        {
            var ledger = await GetOrCreateLedgerAsync(userId);
            ledger.Amount += amountChange;

            if (ledger.Amount < 0)
            {
                throw new InvalidOperationException($"Transaction would result in negative balance for user {userId}");
            }

            _context.Ledgers.Update(ledger);
        }

        /// <summary>
        /// Gets or creates a ledger for a user
        /// </summary>
        private async Task<Ledger> GetOrCreateLedgerAsync(int userId)
        {
            var ledger = await _context.Ledgers
                .FirstOrDefaultAsync(l => l.UserId == userId);

            if (ledger == null)
            {
                ledger = new Ledger
                {
                    UserId = userId,
                    Amount = 0
                };
                _context.Ledgers.Add(ledger);
                await _context.SaveChangesAsync();
            }

            return ledger;
        }
    }
}
