using AccessRequestHub.Common.DTOs;

namespace AccessRequestHub.API.Interfaces
{
    public interface IUserService
    {
        public Task<ServiceResult<IEnumerable<GetUserDto>>> GetAllUsersAsync();
    }
}
