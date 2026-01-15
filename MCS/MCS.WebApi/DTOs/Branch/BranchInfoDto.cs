namespace MCS.WebApi.DTOs.Branch
{
    public class BranchInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? ZipCode { get; set; }
        public string? PhoneNumber { get; set; }
    }
}
