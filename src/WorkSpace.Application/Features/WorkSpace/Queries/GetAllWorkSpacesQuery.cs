using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.Application.Features.WorkSpace.Queries;


public record GetAllWorkSpacesQuery(bool? IsVerified = null) : IRequest<IEnumerable<WorkSpaceModerationDto>>;

public class GetAllWorkSpacesHandler(
    IWorkSpaceRepository repository,
    IMapper mapper)
    : IRequestHandler<GetAllWorkSpacesQuery, IEnumerable<WorkSpaceModerationDto>>
{
    public async Task<IEnumerable<WorkSpaceModerationDto>> Handle(
        GetAllWorkSpacesQuery request,
        CancellationToken cancellationToken)
    {

        var (workSpaces, _) = await repository.GetAllWorkSpacesForAdminAsync(
            1,
            int.MaxValue,
            request.IsVerified,
            cancellationToken);

        return mapper.Map<IEnumerable<WorkSpaceModerationDto>>(workSpaces);
    }
}