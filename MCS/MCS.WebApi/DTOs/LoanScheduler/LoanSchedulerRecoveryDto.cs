using System;

namespace MCS.WebApi.DTOs.LoanScheduler
{
    /// <summary>
    /// Row returned to the Recovery Posting screen for one loan's next unpaid installment.
    /// </summary>
    public class LoanSchedulerRecoveryDto
    {
        public int LoanSchedulerId { get; set; }

        public int LoanId { get; set; }

        public int MemberId { get; set; }

        public string MemberName { get; set; } = string.Empty;

        public string CenterName { get; set; } = string.Empty;

        /// <summary>
        /// Parent POC name (for display in grid).
        /// </summary>
        public string ParentPocName { get; set; } = string.Empty;

        public DateTime ScheduleDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        public int InstallmentNo { get; set; }

        public decimal InterestAmount { get; set; }

        public decimal PrincipalAmount { get; set; }

        public decimal PaymentAmount { get; set; }

        public string Status { get; set; } = "Not Paid";

        /// <summary>
        /// Due amount shown in grid. For now this is the scheduled PaymentAmount.
        /// </summary>
        public decimal Due { get; set; }

        public decimal? ActualEmiAmount { get; set; }

        public decimal? ActualInterestAmount { get; set; }

        public decimal? ActualPrincipalAmount { get; set; }

        public string? Comments { get; set; }

        /// <summary>
        /// Principal share of EMI as percentage (e.g. 85.5). Used for partial payment split.
        /// Computed from PrincipalAmount / PaymentAmount when PaymentAmount &gt; 0.
        /// </summary>
        public decimal PrincipalPercentage { get; set; }

        /// <summary>
        /// Interest share of EMI as percentage (e.g. 14.5). Used for partial payment split.
        /// Computed from InterestAmount / PaymentAmount when PaymentAmount &gt; 0.
        /// </summary>
        public decimal InterestPercentage { get; set; }
    }
}

