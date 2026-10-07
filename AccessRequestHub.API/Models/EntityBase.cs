namespace AccessRequestHub.API.Models
{
    public class EntityBase
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }
    }
}
