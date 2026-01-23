using MCS.WebApi.Models;

namespace MCS.WebApi.DTOs
{
    public class CenterDto
    {
        public int CenterId { get; set; }
        public string CenterName { get; set; }
        public int CreatedBy { get; set; }
        public int BranchId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        
    }

}
