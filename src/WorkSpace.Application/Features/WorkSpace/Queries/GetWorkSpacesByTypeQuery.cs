using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.Application.Features.WorkSpace.Queries
{
    public record GetWorkSpacesByTypeQuery(string TypeName) : IRequest<IEnumerable<WorkSpaceListItemDto>>;

    public class GetWorkSpacesByTypeHandler(
        IWorkSpaceRepository repository,
        IMapper mapper) : IRequestHandler<GetWorkSpacesByTypeQuery, IEnumerable<WorkSpaceListItemDto>>
    {
        public async Task<IEnumerable<WorkSpaceListItemDto>> Handle(GetWorkSpacesByTypeQuery request, CancellationToken cancellationToken)
        {
            var workspaces = await repository.GetWorkSpacesByTypeNameAsync(request.TypeName, cancellationToken);
            var dtoList = mapper.Map<IEnumerable<WorkSpaceListItemDto>>(workspaces);
            return dtoList;
        }
    }
}

