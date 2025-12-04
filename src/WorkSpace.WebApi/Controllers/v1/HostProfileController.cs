using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Extensions;
using Microsoft.AspNetCore.Authorization;
using WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;
using WorkSpace.Application.Features.HostProfile.Commands.UpdateHostProfile;
using WorkSpace.Application.Features.HostProfile.Commands.DeleteHostProfile;
using WorkSpace.Application.Features.HostProfile.Queries.GetHostProfileById;
using WorkSpace.Application.Features.HostProfile.Queries.GetAllHostProfiles;
using WorkSpace.Application.Features.HostProfile.Queries.GetHostProfileByUserId; 

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/host-profile")]
[ApiController]
public class HostProfileController : BaseApiController
{

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateHostProfileCommand command)
    {
        var userId = User.GetUserId();
        command.UserId = userId;

        var result = await Mediator.Send(command);
        return Ok(result.Data);
    }

   
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await Mediator.Send(new GetHostProfileByIdQuery(id));
      
        if (!result.Succeeded) return NotFound(result.Message);
        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
            [FromQuery] bool? isVerified = null,
            [FromQuery] string? companyName = null,
            [FromQuery] string? city = null)
    {
        var query = new GetAllHostProfilesQuery
        {
            PageNumber = 1,
            PageSize = int.MaxValue,
            IsVerified = isVerified,
            CompanyName = companyName,
            City = city
        };

        var result = await Mediator.Send(query);
        return Ok(result.Data);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHostProfileCommand command)
    {
        command.Id = id;
      
        command.RequestingUserId = User.GetUserId();

        var result = await Mediator.Send(command);
        if (!result.Succeeded) return BadRequest(result.Message);

        return Ok(result.Data);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await Mediator.Send(new DeleteHostProfileCommand(id));
        return Ok(result.Data);
    }

  
    [HttpGet("me")] 
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = User.GetUserId();

        var query = new GetHostProfileByUserIdQuery(userId);

        var result = await Mediator.Send(query);
        if (!result.Succeeded) return NotFound(result.Message);

        return Ok(result.Data);
    }
}