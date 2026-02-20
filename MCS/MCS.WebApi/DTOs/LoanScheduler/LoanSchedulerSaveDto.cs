using System;
using System.ComponentModel.DataAnnotations;

namespace MCS.WebApi.DTOs.LoanScheduler
{
    /// <summary>
    /// Single installment update payload used for both single and bulk save.
    /// </summary>
    public class LoanSchedulerSaveDto
    {
        [Required]
        public int LoanSchedulerId { get; set; }

        /// <summary>
        /// Optional status from client (Paid / Not Paid / Partial). Server will enforce
        /// its own rules based on amounts and may override this.
        /// </summary>
        public string? Status { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentMode { get; set; } = string.Empty;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ActualEmiAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ActualInterestAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal ActualPrincipalAmount { get; set; }

        /// <summary>Required when Status is Partial Paid; optional when Paid.</summary>
        [StringLength(500)]
        public string? Comments { get; set; }

        /// <summary>
        /// Optional explicit CollectedBy; if null, server will default to current user.
        /// </summary>
        public int? CollectedBy { get; set; }
    }
}

