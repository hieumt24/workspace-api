using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.Application.Features.WorkSpace.Queries;
public record GetPendingWorkSpacesQuery() : IRequest<IEnumerable<WorkSpaceModerationDto>>;

public class GetPendingWorkSpacesHandler(
    IWorkSpaceRepository repository,
    IMapper mapper)
    : IRequestHandler<GetPendingWorkSpacesQuery, IEnumerable<WorkSpaceModerationDto>>
{
    public async Task<IEnumerable<WorkSpaceModerationDto>> Handle(
        GetPendingWorkSpacesQuery request,
        CancellationToken cancellationToken)
    {

        var (workSpaces, _) = await repository.GetPendingWorkSpacesAsync(
            1,
            int.MaxValue,
            cancellationToken);

        return mapper.Map<IEnumerable<WorkSpaceModerationDto>>(workSpaces);
    }
}