using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.Application.Features.WorkSpace.Queries
{
    public record GetWorkSpacesByTypeIdQuery(int TypeId) : IRequest<IEnumerable<WorkSpaceListItemDto>>;

    public class GetWorkSpacesByTypeIdHandler(
        IWorkSpaceRepository repository,
        IMapper mapper) : IRequestHandler<GetWorkSpacesByTypeIdQuery, IEnumerable<WorkSpaceListItemDto>>
    {
        public async Task<IEnumerable<WorkSpaceListItemDto>> Handle(GetWorkSpacesByTypeIdQuery request, CancellationToken cancellationToken)
        {
            var workspaces = await repository.GetWorkSpacesByTypeIdAsync(request.TypeId, cancellationToken);
            var dtoList = mapper.Map<IEnumerable<WorkSpaceListItemDto>>(workspaces);
            return dtoList;
        }
    }
}

