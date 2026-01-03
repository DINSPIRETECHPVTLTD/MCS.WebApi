using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCS.WebApi.Models
{
    public class Guardian
    {
        [Key]
        public int GuardianId { get; set; }

        [Required]
        public int MemberId { get; set; }

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

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; } = null!;
    }
}

