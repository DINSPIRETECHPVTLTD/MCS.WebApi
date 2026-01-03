using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    public class Center
    {
        [Key]
        public int CenterId { get; set; }

        [Required]
        public int BranchId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("BranchId")]
        public virtual Branch Branch { get; set; } = null!;
        public virtual ICollection<Member> Members { get; set; } = new List<Member>();
    }
}

