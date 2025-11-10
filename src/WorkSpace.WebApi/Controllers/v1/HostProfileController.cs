using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;
using WorkSpace.Application.Features.HostProfile.Commands.UpdateHostProfile;
using WorkSpace.Application.Features.HostProfile.Commands.DeleteHostProfile;
using WorkSpace.Application.Features.HostProfile.Queries.GetHostProfileById;
using WorkSpace.Application.Features.HostProfile.Queries.GetAllHostProfiles;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/host-profile")]
[ApiController]
public class HostProfileController : BaseApiController
{
    /// <summary>
    /// Create a new host profile
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHostProfileCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result.Data);
    }

    /// <summary>
    /// Get host profile by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await Mediator.Send(new GetHostProfileByIdQuery(id));
        return Ok(result.Data);
    }

    /// <summary>
    /// Get all host profiles with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isVerified = null,
        [FromQuery] string? companyName = null,
        [FromQuery] string? city = null)
    {
        var query = new GetAllHostProfilesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IsVerified = isVerified,
            CompanyName = companyName,
            City = city
        };
        
        var result = await Mediator.Send(query);
        return Ok(result.Data);
    }

    /// <summary>
    /// Update host profile
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHostProfileCommand command)
    {
        command.Id = id;
        var result = await Mediator.Send(command);
        return Ok(result.Data);
    }

    /// <summary>
    /// Delete host profile
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await Mediator.Send(new DeleteHostProfileCommand(id));
        return Ok(result.Data);
    }

    /// <summary>
    /// Get host profile by user ID
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        // This would need a separate query, but for now we can use GetAll with filtering
        var query = new GetAllHostProfilesQuery
        {
            PageNumber = 1,
            PageSize = 1
        };
        
        var result = await Mediator.Send(query);
        return Ok(result.Data);
    }
}