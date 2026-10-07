using System.ComponentModel.DataAnnotations;

namespace AccessRequestHub.API.DTOs
{
    public class CreateAccessRequestDto
    {
        [Required]
        public string ClientRequestId { get; set; } = string.Empty;

        [Required]
        public int ApplicationId { get; set; }

        [Required]
        public string Environment { get; set; } = string.Empty;

        [Required]
        public string AccessLevel { get; set; } = string.Empty;

        [Required]
        public string Justification { get; set; } = string.Empty;
    }

    public class GetAccessRequestDto
    {
        public int Id { get; set; }
        public string ClientRequestId { get; set; } = string.Empty;
        public string RequesterEmail { get; set; } = string.Empty;
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; } = string.Empty;
        public string Environment { get; set; } = string.Empty;
        public string AccessLevel { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string PolicyVersion { get; set; } = string.Empty;
        public int Version { get; set; }
        public string? RejectReason { get; set; }
    }

    public class ApprovalActionDto
    {
        [Required]
        public string ActorEmail { get; set; } = string.Empty;

        public string? Reason { get; set; }
    }
}
