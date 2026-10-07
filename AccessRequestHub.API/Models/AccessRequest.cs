using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AccessRequestHub.API.Models
{
    [Table("access_requests")]
    public class AccessRequest : EntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string ClientRequestId { get; set; } = string.Empty;

        [Required]
        public string RequesterEmail { get; set; } = string.Empty;

        [ForeignKey("RequesterEmail")]
        public virtual User? Requester { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public virtual Application? Application { get; set; }

        [Required]
        public string Environment { get; set; } = string.Empty; // NonProduction atau Production

        [Required]
        public string AccessLevel { get; set; } = string.Empty; // Read atau Admin

        [Required]
        public string Justification { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "PendingManager"; // PendingManager, PendingSystemOwner, Approved, Rejected

        [Required]
        public string PolicyVersion { get; set; } = "v1";

        [ConcurrencyCheck]
        public int Version { get; set; } = 1;

        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
