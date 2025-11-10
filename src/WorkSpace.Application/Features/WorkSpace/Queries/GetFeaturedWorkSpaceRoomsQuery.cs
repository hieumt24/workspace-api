using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.WorkSpace.Queries
{
    public record GetFeaturedWorkSpaceRoomsQuery(int Count = 5) : IRequest<Response<IEnumerable<WorkSpaceRoomListItemDto>>>;

    public class GetFeaturedWorkSpaceRoomsHandler(
        IWorkSpaceRepository repository,
        IMapper mapper) : IRequestHandler<GetFeaturedWorkSpaceRoomsQuery, Response<IEnumerable<WorkSpaceRoomListItemDto>>>
    {
        public async Task<Response<IEnumerable<WorkSpaceRoomListItemDto>>> Handle(GetFeaturedWorkSpaceRoomsQuery request, CancellationToken cancellationToken)
        {
            var rooms = await repository.GetFeaturedRoomsAsync(request.Count, cancellationToken);
            var dtoList = mapper.Map<IEnumerable<WorkSpaceRoomListItemDto>>(rooms);

            return new Response<IEnumerable<WorkSpaceRoomListItemDto>>(dtoList)
            {
                Message = $"Found {dtoList.Count()} featured workspace rooms.",
                Succeeded = true
            };
        }
    }
}

