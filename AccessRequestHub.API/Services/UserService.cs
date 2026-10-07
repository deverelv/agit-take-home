using AccessRequestHub.API.Context;
using AccessRequestHub.API.Interfaces;
using AccessRequestHub.Common.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestHub.API.Services
{
    public class UserService(AppDbContext context) : IUserService
    {
        public async Task<ServiceResult<IEnumerable<GetUserDto>>> GetAllUsersAsync()
        {
            var users = await context.Users
                .Select(user => new GetUserDto
                {
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role,
                    ManagerEmail = user.ManagerEmail
                })
                .ToListAsync();

            return ServiceResult<IEnumerable<GetUserDto>>.Ok(users);
        }
    }
}
