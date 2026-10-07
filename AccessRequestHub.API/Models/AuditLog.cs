using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AccessRequestHub.API.Models
{
    [Table("audit_logs")]
    public class AuditLog : EntityBase
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int RequestId { get; set; }

        [ForeignKey("RequestId")]
        public virtual AccessRequest? AccessRequest { get; set; }

        [Required]
        public string ActorEmail { get; set; } = string.Empty;

        [ForeignKey("ActorEmail")]
        public virtual User? Actor { get; set; }

        [Required]
        public string Action { get; set; } = string.Empty; // CREATED, MANAGER_APPROVED, SYSTEM_OWNER_APPROVED, REJECTED

        public string? Details { get; set; }
    }
}
