using AccessRequestHub.Common.DTOs;

namespace AccessRequestHub.API.Interfaces
{
    public interface IApplicationService
    {
        public Task<ServiceResult<IEnumerable<GetApplicationDto>>> GetAllApplicationsAsync();
    }
}
