using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.Promotions.Queries;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/promotions")]
    public class PromotionController : BaseApiController
    {
        [HttpGet("active")]
        public async Task<IActionResult> GetActivePromotions(
            CancellationToken cancellationToken = default)
        {
            var result = await Mediator.Send(new GetActivePromotionsQuery(), cancellationToken);
            return Ok(result);
        }
    }
}

