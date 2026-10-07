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
            return await ProcessActionAsync(requestId, dto, "approve");
        }

        public async Task<ServiceResult<GetAccessRequestDto>> RejectRequestAsync(int requestId, ApprovalActionDto dto)
        {
            return await ProcessActionAsync(requestId, dto, "reject");
        }

        private async Task<ServiceResult<GetAccessRequestDto>> ProcessActionAsync(int requestId, ApprovalActionDto dto, string actionType)
        {
            string actionLower = actionType.ToLower();
            string pastTense = actionLower == "approve" ? "approved" : "rejected";
            string actionNoun = actionLower == "approve" ? "approval" : "rejection";

            if (actionLower == "reject" && string.IsNullOrWhiteSpace(dto.Reason))
            {
                logger.LogWarning("Reject attempt without reason. RequestId: {RequestId}, ActorEmail: {ActorEmail}", requestId, dto.ActorEmail);
                return ServiceResult<GetAccessRequestDto>.Fail("A reason is required when rejecting an access request.");
            }

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
                logger.LogWarning("Cannot {Action} access request because it is already in a terminal state. RequestId: {RequestId}, Status: {Status}",
                    actionLower, requestId, request.Status);
                return ServiceResult<GetAccessRequestDto>.Fail("Access request is already in a terminal state.");
            }

            var actor = await context.Users.FindAsync(dto.ActorEmail);
            if (actor == null)
            {
                logger.LogWarning("Actor not found. ActorEmail: {ActorEmail}, RequestId: {RequestId}", dto.ActorEmail, requestId);
                return ServiceResult<GetAccessRequestDto>.Fail($"{char.ToUpper(actionNoun[0]) + actionNoun[1..]} actor not found.");
            }

            if (request.RequesterEmail == actor.Email)
            {
                logger.LogWarning("Self-{Action} attempt detected. RequestId: {RequestId}, ActorEmail: {ActorEmail}", actionLower, requestId, actor.Email);
                return ServiceResult<GetAccessRequestDto>.Fail($"You cannot {actionLower} your own access request.");
            }

            if (request.Status == "PendingManager")
            {
                if (request.Requester?.ManagerEmail != actor.Email)
                {
                    logger.LogWarning("Unauthorized manager {Action} attempt. RequestId: {RequestId}, ActorEmail: {ActorEmail}, ExpectedManagerEmail: {ManagerEmail}",
                        actionLower, requestId, actor.Email, request.Requester?.ManagerEmail);
                    return ServiceResult<GetAccessRequestDto>.Fail($"You are not authorized to {actionLower} this request as its manager.");
                }
            }
            else if (request.Status == "PendingSystemOwner")
            {
                if (request.Application?.SystemOwnerEmail != actor.Email)
                {
                    logger.LogWarning("Unauthorized system owner {Action} attempt. RequestId: {RequestId}, ActorEmail: {ActorEmail}, ExpectedOwnerEmail: {OwnerEmail}",
                        actionLower, requestId, actor.Email, request.Application?.SystemOwnerEmail);
                    return ServiceResult<GetAccessRequestDto>.Fail($"You are not authorized to {actionLower} this request as the system owner.");
                }
            }
            else
            {
                logger.LogWarning("Invalid access request status for {Action}. RequestId: {RequestId}, Status: {Status}",
                    actionLower, requestId, request.Status);
                return ServiceResult<GetAccessRequestDto>.Fail($"Access request cannot be {pastTense} from its current status.");
            }

            string auditAction = "";

            if (actionLower == "approve")
            {
                if (request.Status == "PendingManager")
                {
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
                    request.Status = "Approved";
                    auditAction = "SYSTEM_OWNER_APPROVED_FINAL";
                }
            }
            else // reject
            {
                request.Status = "Rejected";
                auditAction = "REJECTED";
            }

            request.ModifiedDate = DateTime.UtcNow;
            request.Version++;

            try
            {
                await context.SaveChangesAsync();
                string auditDetails = actionLower == "approve" ? $"Approved by {actor.Email}" : $"Reason: {dto.Reason}";
                await LogAuditAsync(request.Id, actor.Email, auditAction, auditDetails);
            }
            catch (DbUpdateConcurrencyException)
            {
                logger.LogError("Concurrency conflict while {Action}ing access request. RequestId: {RequestId}, ActorEmail: {ActorEmail}",
                    actionLower, requestId, actor.Email);
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
