using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Support;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.SupportTickets.Commands;
using WorkSpace.Domain.Enums;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/support")]
    [ApiController]
    [Authorize(Roles = $"{nameof(Roles.Owner)},{nameof(Roles.Customer)}")]
    public class SupportController : BaseApiController
    {
        [HttpPost("tickets")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest request)
        {
            var userId = User.GetUserId();
            if (userId == 0) return Unauthorized();

           
            if (request.TicketType == SupportTicketType.Feedback && !User.IsInRole(nameof(Roles.Owner)))
            {
                return Forbid("Only Owners can submit feedback.");
            }

            var command = new CreateTicketCommand
            {
                Dto = request,
                SubmittedByUserId = userId
            };

            var result = await Mediator.Send(command);
            return Ok(result);
        }

     
    }
}