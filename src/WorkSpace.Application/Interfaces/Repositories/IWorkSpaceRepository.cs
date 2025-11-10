
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories
{
    public interface IWorkSpaceRepository : IGenericRepositoryAsync<Domain.Entities.WorkSpace>
    {
        Task<WorkSpace.Domain.Entities.WorkSpace?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<WorkSpaceRoom> Rooms, int TotalCount)> GetRoomsPagedAsync(
            WorkSpaceFilter filter,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default
        );

        Task<bool> ExistsTitleForHostAsync(int hostId, string title, CancellationToken cancellationToken = default);

        Task<WorkSpaceRoom?> GetRoomByIdWithDetailsAsync(int roomId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<WorkSpaceRoom>> GetFeaturedRoomsAsync(int count = 5, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Domain.Entities.WorkSpace>> GetWorkSpacesByTypeNameAsync(string typeName, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Domain.Entities.WorkSpace>> GetWorkSpacesByTypeIdAsync(int typeId, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<WorkSpaceRoom> Rooms, int TotalCount)> GetAvailableRoomsAsync(
            CheckAvailableRoomsRequestInternal request,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default
        );

        Task<(IReadOnlyList<Domain.Entities.WorkSpace> WorkSpaces, int TotalCount)> GetPendingWorkSpacesAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default
        );

        Task<(IReadOnlyList<Domain.Entities.WorkSpace> WorkSpaces, int TotalCount)> GetAllWorkSpacesForAdminAsync(
            int pageNumber,
            int pageSize,
            bool? isVerified,
            CancellationToken cancellationToken = default
        );
    }
}