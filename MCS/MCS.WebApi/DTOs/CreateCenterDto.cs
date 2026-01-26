using System.ComponentModel.DataAnnotations;

namespace MCS.WebApi.DTOs
{
    public class CreateCenterDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? CenterAddress { get; set; }
      
        
    }
}
