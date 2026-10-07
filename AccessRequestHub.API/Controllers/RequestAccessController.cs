using AccessRequestHub.API.DTOs;
using AccessRequestHub.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestHub.API.Controllers
{
    [ApiController]
    [Route("api/request-access")]
    public class RequestAccessController(IRequestAccessService service) : ControllerBase
    {
        [HttpPost("create")]
        public async Task<IActionResult> CreateNewRequest([FromHeader(Name = "X-User-Email")] string requesterEmail, [FromBody] CreateAccessRequestDto dto)
        {
            try
            {
                var result = await service.CreateNewRequestAsync(requesterEmail, dto);

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
