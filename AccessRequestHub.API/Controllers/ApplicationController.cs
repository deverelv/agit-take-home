using AccessRequestHub.API.Context;
using AccessRequestHub.API.Interfaces;
using AccessRequestHub.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestHub.API.Controllers
{
    [ApiController]
    [Route("api/application")]
    public class ApplicationController(IApplicationService service) : ControllerBase
    {
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllApplications()
        {
            try
            {
                var result = await service.GetAllApplicationsAsync();

                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
