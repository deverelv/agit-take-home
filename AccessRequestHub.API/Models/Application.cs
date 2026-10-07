using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AccessRequestHub.API.Models
{
    [Table("applications")]
    public class Application
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string SystemOwnerEmail { get; set; } = string.Empty;

        [ForeignKey("SystemOwnerEmail")]
        public virtual User? SystemOwner { get; set; }

        public virtual ICollection<AccessRequest> AccessRequests { get; set; } = new List<AccessRequest>();
    }
}
