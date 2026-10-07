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

            await LogAuditAsync(newRequest.Id, requesterEmail, "CREATED", "Request access created.");

            return ServiceResult<GetAccessRequestDto>.Ok(newRequest.ToDto());
        }

        public async Task<ServiceResult<GetAccessRequestDto>> ApproveRequestAsync(int requestId, ApprovalActionDto dto)
        {
            var request = await context.AccessRequests
                .Include(r => r.Application)
                .Include(r => r.Requester)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
            {
                logger.LogWarning("Access request not found. RequestId: {RequestId}", requestId);
                return ServiceResult<GetAccessRequestDto>.Fail("Access request not found.");
            }

            if (request.Status == "Approved" || request.Status == "Rejected")
            {
                logger.LogWarning("Cannot approve access request because it is already in a terminal state. " +
                    "Id: {RequestId}, Status: {Status}", requestId, request.Status);
                return ServiceResult<GetAccessRequestDto>.Fail("Access request is already in a terminal state.");
            }

            var actor = await context.Users.FindAsync(dto.ActorEmail);
            if (actor == null)
            {
                logger.LogWarning("Approval actor not found. ActorEmail: {ActorEmail}, RequestId: {RequestId}", dto.ActorEmail, requestId);
                return ServiceResult<GetAccessRequestDto>.Fail("Approval actor not found.");
            }

            if (request.RequesterEmail == actor.Email)
            {
                logger.LogWarning("Self-approval attempt detected. RequestId: {RequestId}, ActorEmail: {ActorEmail}", requestId, actor.Email);
                return ServiceResult<GetAccessRequestDto>.Fail("You cannot approve your own access request.");
            }

            string auditAction = "";

            if (request.Status == "PendingManager")
            {
                if (request.Requester?.ManagerEmail != actor.Email && actor.Role != "Manager")
                {
                    logger.LogWarning("Unauthorized manager approval attempt. RequestId: {RequestId}, " +
                        "ActorEmail: {ActorEmail}, ExpectedManagerEmail: {ManagerEmail}", requestId, actor.Email, request.Requester?.ManagerEmail);
                    throw new UnauthorizedAccessException("You are not authorized to approve this request as its manager.");
                }

                bool isHighRisk = request.Environment == "Production" || request.AccessLevel == "Admin";

                if (isHighRisk)
                {
                    request.Status = "PendingSystemOwner";
                    auditAction = "MANAGER_APPROVED_PENDING_OWNER";
                }
                else
                {
                    request.Status = "Approved";
                    auditAction = "MANAGER_APPROVED_FINAL";
                }
            }
            else if (request.Status == "PendingSystemOwner")
            {
                if (request.Application?.SystemOwnerEmail != actor.Email)
                {
                    logger.LogWarning("Unauthorized system owner approval attempt. RequestId: {RequestId}, " +
                        "ActorEmail: {ActorEmail}, ExpectedOwnerEmail: {OwnerEmail}"
                        , requestId, actor.Email, request.Application?.SystemOwnerEmail);
                    throw new UnauthorizedAccessException("You are not authorized to approve this request as the system owner.");
                }

                request.Status = "Approved";
                auditAction = "SYSTEM_OWNER_APPROVED_FINAL";
            }
            else
            {
                logger.LogWarning("Invalid access request status for approval. RequestId: {RequestId}, Status: {Status}",
                    requestId, request.Status);
                return ServiceResult<GetAccessRequestDto>.Fail("Access request cannot be approved from its current status.");
            }

            request.ModifiedDate = DateTime.UtcNow;
            request.Version++;

            try
            {
                await context.SaveChangesAsync();
                await LogAuditAsync(request.Id, actor.Email, auditAction, $"Approved by {actor.Email}");
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError("Concurrency conflict while approving access request. " +
                    "RequestId: {RequestId}, ActorEmail: {ActorEmail}", requestId, actor.Email);
                throw new InvalidOperationException("The access request was modified by another process. Please reload the request and try again.");
            }

            return ServiceResult<GetAccessRequestDto>.Ok(request.ToDto());
        }

        private async Task LogAuditAsync(int requestId, string actorEmail, string action, string details)
        {
            var audit = new AuditLog
            {
                RequestId = requestId,
                ActorEmail = actorEmail,
                Action = action,
                Details = details,
                CreatedDate = DateTime.UtcNow
            };
            context.AuditLogs.Add(audit);
            await context.SaveChangesAsync();
        }
    }
}
