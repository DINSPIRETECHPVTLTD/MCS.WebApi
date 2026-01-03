using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    public class Member
    {
        [Key]
        public int MemberId { get; set; }

        [Required]
        public int BranchId { get; set; }

        public int? CenterId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? DOB { get; set; }

        public int? Age { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(12)]
        public string? Aadhaar { get; set; }

        [StringLength(100)]
        public string? Occupation { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; } = null!;

        [ForeignKey("CenterId")]
        public virtual Center? Center { get; set; }

        public virtual ICollection<Guardian> Guardians { get; set; } = new List<Guardian>();
    }
}

