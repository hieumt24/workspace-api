using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories
{
    public interface IWorkSpaceFavoriteRepository 
    {
        Task<bool> AddToFavoritesAsync(int workSpaceId, int userId);
        Task<bool> RemoveFromFavoritesAsync(int workSpacelId, int userId);
        Task<bool> IsFavoriteAsync(int workSpaceId, int userId);
        Task<List<int>> GetFavoriteWorkSpaceIdsAsync(int userId);
        Task<List<WorkSpace.Domain.Entities.WorkSpace>> GetFavoriteWorkSpacesAsync(int userId);
    }
}