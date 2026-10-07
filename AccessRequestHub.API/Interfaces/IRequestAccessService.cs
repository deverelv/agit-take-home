using AccessRequestHub.Common.DTOs;
using AccessRequestHub.API.Models;

namespace AccessRequestHub.API.Interfaces
{
    public interface IRequestAccessService
    {
        public Task<ServiceResult<GetAccessRequestDto>> CreateNewRequestAsync(string requesterEmail, CreateAccessRequestDto dto);
        public Task<ServiceResult<GetAccessRequestDto>> ApproveRequestAsync(int requestId, ApprovalActionDto dto);

        public Task<ServiceResult<GetAccessRequestDto>> RejectRequestAsync(int requestId, ApprovalActionDto dto);
    }
}
