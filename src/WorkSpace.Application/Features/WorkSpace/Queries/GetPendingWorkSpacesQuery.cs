using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Parameters;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.WorkSpace.Queries;

public record GetPendingWorkSpacesQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResponse<IEnumerable<WorkSpaceModerationDto>>>;

public class GetPendingWorkSpacesHandler(
    IWorkSpaceRepository repository,
    IMapper mapper) 
    : IRequestHandler<GetPendingWorkSpacesQuery, PagedResponse<IEnumerable<WorkSpaceModerationDto>>>
{
    public async Task<PagedResponse<IEnumerable<WorkSpaceModerationDto>>> Handle(
        GetPendingWorkSpacesQuery request,
        CancellationToken cancellationToken)
    {
        var (workSpaces, totalCount) = await repository.GetPendingWorkSpacesAsync(
            request.PageNumber, 
            request.PageSize, 
            cancellationToken);

        var dtoList = mapper.Map<IEnumerable<WorkSpaceModerationDto>>(workSpaces);

        var pagedResponse = new PagedResponse<IEnumerable<WorkSpaceModerationDto>>(
            dtoList, 
            request.PageNumber, 
            request.PageSize)
        {
            Message = $"Tìm thấy {totalCount} workspace chờ duyệt."
        };
        
        return pagedResponse;
    }
}

