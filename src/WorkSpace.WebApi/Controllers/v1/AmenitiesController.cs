using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.Amenities.Queries;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/amenities")]
public class AmenitiesController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var amenities = await Mediator.Send(new GetAllAmenitiesQuery(), cancellationToken);
        return Ok(amenities);
    }
}
