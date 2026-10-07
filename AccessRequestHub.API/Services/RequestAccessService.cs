using AccessRequestHub.API.Context;
using AccessRequestHub.API.DTOs;
using AccessRequestHub.API.Models;
using Microsoft.EntityFrameworkCore;
using AccessRequestHub.API.Interfaces;
using AccessRequestHub.API.Extensions;

namespace AccessRequestHub.API.Services
{
    public class RequestAccessService(ILogger<RequestAccessService> logger, AppDbContext context) : IRequestAccessService
    {
        public async Task<ServiceResult<GetAccessRequestDto>> CreateNewRequestAsync(string requesterEmail, CreateAccessRequestDto dto)
        {
            var existingRequest = await context.AccessRequests
                .Include(r => r.Application)
                .Include(r => r.Requester)
                .FirstOrDefaultAsync(r => r.ClientRequestId == dto.ClientRequestId); // Idempotency check
            if (existingRequest != null)
            {
                logger.LogWarning("Duplicate access request. Id: {ClientRequestId}, Requester: {RequesterEmail}", dto.ClientRequestId, requesterEmail);
                return ServiceResult<GetAccessRequestDto>.Fail("Duplicate request.");
            }
            

            var requester = await context.Users.FindAsync(requesterEmail);
            if (requester == null)
            {
                logger.LogWarning("Requester not found. Id: {RequesterEmail}", requesterEmail);
                return ServiceResult<GetAccessRequestDto>.Fail("Requester not found.");
            }

            var app = await context.Applications.FindAsync(dto.ApplicationId);
            if (app == null)
            {
                logger.LogWarning("Application not found. Id: {ApplicationId}", dto.ApplicationId);
                return ServiceResult<GetAccessRequestDto>.Fail("Application not found.");
            }

            var newRequest = new AccessRequest
            {
                ClientRequestId = dto.ClientRequestId,
                RequesterEmail = requesterEmail,
                ApplicationId = dto.ApplicationId,
                Environment = dto.Environment,
                AccessLevel = dto.AccessLevel,
                Justification = dto.Justification,
                Status = "PendingManager",
                PolicyVersion = "v1",
                Version = 1,
                CreatedDate = DateTime.UtcNow
            };

            context.AccessRequests.Add(newRequest);
            await context.SaveChangesAsync();

            // await LogAuditAsync(newRequest.Id, requesterEmail, "CREATED", "Permohonan akses berhasil diajukan.");

            return ServiceResult<GetAccessRequestDto>.Ok(newRequest.ToDto());
        }
    }
}
