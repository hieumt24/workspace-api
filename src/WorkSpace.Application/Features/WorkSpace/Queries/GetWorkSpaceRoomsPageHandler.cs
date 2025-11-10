using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.WorkSpace.Queries
{
    // Đổi tên record để phản ánh việc query Rooms
    public record GetWorkSpaceRoomsPagedQuery(
        WorkSpaceFilter filter,
        int pageNumber = 1,
        int pageSize = 10
    ) : IRequest<PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>>;

    // Đổi tên class Handler và các kiểu dữ liệu liên quan
    public class GetWorkSpaceRoomsPageHandler(
        IWorkSpaceRepository repository,
        IMapper mapper) : IRequestHandler<GetWorkSpaceRoomsPagedQuery, PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>>
    {
        public async Task<PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>> Handle(GetWorkSpaceRoomsPagedQuery request,
            CancellationToken cancellationToken)
        {
            // SỬA LỖI TẠI ĐÂY: Gọi đúng phương thức GetRoomsPagedAsync
            var (rooms, count) = await repository.GetRoomsPagedAsync(request.filter, request.pageNumber, request.pageSize, cancellationToken);

            // Map kết quả sang DTO mới
            var dtoList = mapper.Map<IEnumerable<WorkSpaceRoomListItemDto>>(rooms);

            var pagedResponse = new PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>(dtoList, request.pageNumber, request.pageSize)
            {
                Message = $"Found {count} records."
            };
            return pagedResponse;
        }
    }
}