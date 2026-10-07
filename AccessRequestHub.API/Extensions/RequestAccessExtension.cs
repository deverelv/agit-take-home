using AccessRequestHub.API.DTOs;
using AccessRequestHub.API.Models;

namespace AccessRequestHub.API.Extensions
{
    public static class RequestAccessExtension
    {
        public static GetAccessRequestDto ToDto(this AccessRequest entity)
        {
            if (entity == null) return null!;

            return new GetAccessRequestDto
            {
                Id = entity.Id,
                ClientRequestId = entity.ClientRequestId,
                RequesterEmail = entity.RequesterEmail,
                ApplicationId = entity.ApplicationId,
                ApplicationName = entity.Application?.Name ?? string.Empty,
                Environment = entity.Environment,
                AccessLevel = entity.AccessLevel,
                Justification = entity.Justification,
                Status = entity.Status,
                PolicyVersion = entity.PolicyVersion,
                Version = entity.Version
            };
        }
    }
}
