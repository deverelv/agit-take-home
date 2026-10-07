namespace AccessRequestHub.Common.DTOs
{
    public class GetApplicationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SystemOwnerEmail { get; set; } = string.Empty;
    }
}
