using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        [Required]
        public int OrganizationId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("OrganizationId")]
        public virtual Organization Organization { get; set; } = null!;
        public virtual ICollection<BranchUser> BranchUsers { get; set; } = new List<BranchUser>();
        public virtual ICollection<Center> Centers { get; set; } = new List<Center>();
        public virtual ICollection<Member> Members { get; set; } = new List<Member>();
    }
}

