using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AccessRequestHub.API.Interfaces;

namespace AccessRequestHub.API.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController(IUserService service) : ControllerBase
    {
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var result = await service.GetAllUsersAsync();

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
