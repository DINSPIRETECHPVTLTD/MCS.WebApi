using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace MCS.WebApi.Models
{
    [Table("Centers", Schema = "dinspire_mfdev")]
    public class Center
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int BranchId { get; set; }

        public string? CenterAddress { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        [Required]
        public bool IsDeleted { get; set; }
       // public int IsDeleted { get; set; }

        // Navigation properties - ADD JsonIgnore to prevent validation errors
        [ForeignKey("BranchId")]
        [JsonIgnore]  // ADD THIS
        public virtual Branch? Branch { get; set; }  // Make nullable

        [ForeignKey("CreatedBy")]
        [JsonIgnore]  // ADD THIS
        public virtual User? CreatedByUser { get; set; }

        [ForeignKey("ModifiedBy")]
        [JsonIgnore]  // ADD THIS
        public virtual User? ModifiedByUser { get; set; }

        [JsonIgnore]  // ADD THIS
        public virtual ICollection<POC> POCs { get; set; } = new List<POC>();

        [JsonIgnore]  // ADD THIS
        public virtual ICollection<Member> Members { get; set; } = new List<Member>();
    }
}