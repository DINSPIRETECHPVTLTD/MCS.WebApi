using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    [Table("MemberMembershipFees")]
    public class MemberMembershipFee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MemberId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime? PaidDate { get; set; }

        public int? CollectedBy { get; set; }

        [StringLength(50)]
        public string? PaymentMode { get; set; } 

        [StringLength(500)]
        public string? Comments { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        [Required]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; } = null!;

        [ForeignKey("CreatedBy")]
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey("ModifiedBy")]
        public virtual User? ModifiedByUser { get; set; }

        [ForeignKey("CollectedBy")]
        public virtual User? CollectedByUser { get; set; }
    }
}
