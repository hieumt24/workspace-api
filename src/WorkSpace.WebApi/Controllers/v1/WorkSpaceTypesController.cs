using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.WorkSpace.Queries;
using WorkSpace.Application.Features.WorkSpaceTypes.Queries; 

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/workspacetypes")]
[ApiController]

public class WorkSpaceTypesController : BaseApiController
{


    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
       
        var result = await Mediator.Send(new GetAllWorkSpaceTypesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}/workspaces")]
    public async Task<IActionResult> GetWorkSpacesByTypeId(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        
        var workspaces = await Mediator.Send(new GetWorkSpacesByTypeIdQuery(id), cancellationToken);

        return Ok(workspaces);
    }
}