using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.WorkSpaceTypes.Queries;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/workspacetypes")]
[ApiController]
public class WorkSpaceTypesController : ControllerBase
{
    private IWorkSpaceTypeRepository _workSpaceTypeRepository;
    public WorkSpaceTypesController(IWorkSpaceTypeRepository workSpaceTypeRepository)
    {
        _workSpaceTypeRepository = workSpaceTypeRepository;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var workspceTypes = await _workSpaceTypeRepository.GetAllWorkSpaceType();
        return Ok(workspceTypes);
    }

    [HttpGet("{id}/workspaces")]
    public async Task<IActionResult> GetWorkSpacesByTypeId([FromRoute] int id)
    {
        var workspaces = await _workSpaceTypeRepository.GetWorkSpacesByTypeId(id);
        return Ok(workspaces);
    }
}

