using AccessRequestHub.API.Context;
using AccessRequestHub.API.Interfaces;
using AccessRequestHub.API.Models;
using AccessRequestHub.Common.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestHub.API.Services
{
    public class ApplicationService(AppDbContext context) : IApplicationService
    {
        public async Task<ServiceResult<IEnumerable<GetApplicationDto>>> GetAllApplicationsAsync()
        {
            var applications = await context.Applications
                .Select(app => new GetApplicationDto
                {
                    Id = app.Id,
                    Name = app.Name,
                    SystemOwnerEmail = app.SystemOwnerEmail
                })
            .ToListAsync();

            return ServiceResult<IEnumerable<GetApplicationDto>>.Ok(applications);
        }
    }
}
