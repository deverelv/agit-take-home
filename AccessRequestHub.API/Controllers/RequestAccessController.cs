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

        [HttpPost("approve/{id}")]
        public async Task<IActionResult> ApproveRequest(int id, [FromBody] ApprovalActionDto dto)
        {
            try
            {
                var result = await service.ApproveRequestAsync(id, dto);
                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reject/{id}")]
        public async Task<IActionResult> RejectRequest(int id, [FromBody] ApprovalActionDto dto)
        {
            try
            {
                var result = await service.RejectRequestAsync(id, dto);
                if (!result.Success)
                {
                    return BadRequest(result.ErrorMessage);
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
