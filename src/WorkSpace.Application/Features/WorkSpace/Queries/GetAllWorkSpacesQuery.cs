using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.WorkSpace.Queries;

public record GetAllWorkSpacesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    bool? IsVerified = null
) : IRequest<PagedResponse<IEnumerable<WorkSpaceModerationDto>>>;

public class GetAllWorkSpacesHandler(
    IWorkSpaceRepository repository,
    IMapper mapper) 
    : IRequestHandler<GetAllWorkSpacesQuery, PagedResponse<IEnumerable<WorkSpaceModerationDto>>>
{
    public async Task<PagedResponse<IEnumerable<WorkSpaceModerationDto>>> Handle(
        GetAllWorkSpacesQuery request,
        CancellationToken cancellationToken)
    {
        var (workSpaces, totalCount) = await repository.GetAllWorkSpacesForAdminAsync(
            request.PageNumber, 
            request.PageSize,
            request.IsVerified,
            cancellationToken);

        var dtoList = mapper.Map<IEnumerable<WorkSpaceModerationDto>>(workSpaces);

        var pagedResponse = new PagedResponse<IEnumerable<WorkSpaceModerationDto>>(
            dtoList, 
            request.PageNumber, 
            request.PageSize)
        {
            Message = $"Tìm thấy {totalCount} workspace."
        };
        
        return pagedResponse;
    }
}

