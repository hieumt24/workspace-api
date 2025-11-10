using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.WorkSpace.Queries;

public record GetAvailableRoomsQuery(
    CheckAvailableRoomsRequestInternal Request,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PagedResponse<IEnumerable<AvailableRoomDto>>>;

public class GetAvailableRoomsQueryHandler(
    IWorkSpaceRepository repository,
    IMapper mapper) : IRequestHandler<GetAvailableRoomsQuery, PagedResponse<IEnumerable<AvailableRoomDto>>>
{
    public async Task<PagedResponse<IEnumerable<AvailableRoomDto>>> Handle(
        GetAvailableRoomsQuery request,
        CancellationToken cancellationToken)
    {
        var (rooms, totalCount) = await repository.GetAvailableRoomsAsync(
            request.Request,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var dtoList = mapper.Map<IEnumerable<AvailableRoomDto>>(rooms);

        var pagedResponse = new PagedResponse<IEnumerable<AvailableRoomDto>>(
            dtoList,
            request.PageNumber,
            request.PageSize)
        {
            Message = $"Found {totalCount} available rooms for the specified time period."
        };

        return pagedResponse;
    }
}

