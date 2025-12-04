using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Promotions;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.Promotions.Commands.ActivatePromotion;
using WorkSpace.Application.Features.Promotions.Commands.GeneratePromotion;
using WorkSpace.Application.Features.Promotions.Queries;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/promotions")]
    [ApiController]
    public class PromotionController : BaseApiController
    {

        [HttpGet("active")]
        public async Task<IActionResult> GetActivePromotions(CancellationToken ct)
        {
            var result = await Mediator.Send(new GetActivePromotionsQuery(), ct);
            return Ok(result);
        }

     
        [HttpPost("admin/generate")]
        [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Staff)}")]
        public async Task<IActionResult> AdminGenerateCode([FromBody] GeneratePromotionDto dto)
        {
            var command = new GeneratePromotionCommand
            {
                Dto = dto,
                RequestUserId = User.GetUserId(),
                IsOwnerCode = false 
            };

            var result = await Mediator.Send(command);

   
            if (!result.Succeeded) return BadRequest(new { error = result.Message });
            return Ok(result.Data);
        }

        [HttpPut("admin/activate/{id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Staff)}")]
        public async Task<IActionResult> AdminActivateCode(int id)
        {
            var command = new ActivatePromotionCommand
            {
                PromotionId = id,
                RequestUserId = User.GetUserId(),
                IsOwnerAction = false
            };

            var result = await Mediator.Send(command);

            if (!result.Succeeded) return BadRequest(new { error = result.Message });
            return Ok(new { success = true, message = "Activated successfully" });
        }

        [HttpPost("owner/generate")]
        [Authorize(Roles = nameof(Roles.Owner))]
        public async Task<IActionResult> OwnerGenerateCode([FromBody] GeneratePromotionDto dto)
        {
            var command = new GeneratePromotionCommand
            {
                Dto = dto,
                RequestUserId = User.GetUserId(),
                IsOwnerCode = true 
            };

            var result = await Mediator.Send(command);

            if (!result.Succeeded) return BadRequest(new { error = result.Message });
            return Ok(result.Data);
        }

     
        [HttpPut("owner/activate/{id}")]
        [Authorize(Roles = nameof(Roles.Owner))]
        public async Task<IActionResult> OwnerActivateCode(int id)
        {
            var command = new ActivatePromotionCommand
            {
                PromotionId = id,
                RequestUserId = User.GetUserId(),
                IsOwnerAction = true
            };

            var result = await Mediator.Send(command);

            if (!result.Succeeded) return BadRequest(new { error = result.Message });
            return Ok(new { success = true, message = "Activated successfully" });
        }
    }
}