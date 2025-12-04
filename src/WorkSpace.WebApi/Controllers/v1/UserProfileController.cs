using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.Users.Queries.GetCurrentUser;
using WorkSpace.Application.Extensions;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/profile")]
    [ApiController]
    [Authorize]
    public class UserProfileController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetCurrentUserInfo(CancellationToken cancellationToken)
        {
            
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized(new { message = "Authentication required" });

            if (Mediator == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Mediator not initialized.");

            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            System.Diagnostics.Debug.WriteLine($"All Claims: {System.Text.Json.JsonSerializer.Serialize(allClaims)}");

            var userId = User.GetUserId();
            if (userId == 0)
            {
                
                return Unauthorized(new { 
                    message = "Invalid user token - userId not found in claims", 
                    claims = allClaims 
                });
            }

            return Ok(await Mediator.Send(new GetCurrentUserQuery { UserId = userId }, cancellationToken));
        }
    }
}

