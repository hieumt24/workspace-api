using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.WorkSpaceTypes;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.WorkSpaceTypes.Queries;

public record GetAllWorkSpaceTypesQuery : IRequest<IEnumerable<WorkSpaceTypeDto>>;

public class GetAllWorkSpaceTypesQueryHandler(
    IGenericRepositoryAsync<WorkSpaceType> repository,
    IMapper mapper) : IRequestHandler<GetAllWorkSpaceTypesQuery, IEnumerable<WorkSpaceTypeDto>>
{
    public async Task<IEnumerable<WorkSpaceTypeDto>> Handle(GetAllWorkSpaceTypesQuery request, CancellationToken cancellationToken)
    {
        var workSpaceTypes = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<IEnumerable<WorkSpaceTypeDto>>(workSpaceTypes);
    }
}

