using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccessRequestHub.API.Models
{
    [Table("users")]
    public class User : EntityBase
    {
        [Key]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty; // Requester, Manager, SystemOwner, Auditor

        [MaxLength(255)]
        public string? ManagerEmail { get; set; }

        [ForeignKey("ManagerEmail")]
        public virtual User? Manager { get; set; }

        public virtual ICollection<User> DirectReports { get; set; } = new List<User>();
        public virtual ICollection<Application> OwnedApplications { get; set; } = new List<Application>();
        public virtual ICollection<AccessRequest> AccessRequests { get; set; } = new List<AccessRequest>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
