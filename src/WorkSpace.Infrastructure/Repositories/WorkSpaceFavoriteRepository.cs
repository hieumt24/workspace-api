using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{

    public class WorkSpaceFavoriteRepository : IWorkSpaceFavoriteRepository
    {
        private readonly WorkSpaceContext _context;

        public WorkSpaceFavoriteRepository(WorkSpaceContext dbContext) 
        {
            _context = dbContext;
        }

        public async Task<bool> AddToFavoritesAsync(int workSpaceId, int userId)
        {
            var favorite = new WorkSpaceFavorite
            {
                WorkspaceId = workSpaceId,
                UserId = userId
            };
            _context.WorkSpaceFavorites.Add(favorite);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<int>> GetFavoriteWorkSpaceIdsAsync(int userId)
        {
            return await _context.WorkSpaceFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.WorkspaceId)
                .ToListAsync();
        }

        public async Task<List<Domain.Entities.WorkSpace>> GetFavoriteWorkSpacesAsync(int userId)
        {
            return await _context.WorkSpaceFavorites
                .Where(f => f.UserId == userId)
                .Select(f => f.Workspace!)
                .ToListAsync();
        }

        public async Task<bool> IsFavoriteAsync(int workSpaceId, int userId)
        {
            return await _context.WorkSpaceFavorites
                .AnyAsync(f => f.WorkspaceId == workSpaceId && f.UserId == userId);
        }

        public async Task<bool> RemoveFromFavoritesAsync(int workSpacelId, int userId)
        {
            var favorite = await _context.WorkSpaceFavorites
                .FirstOrDefaultAsync(f => f.WorkspaceId == workSpacelId && f.UserId == userId);
            if (favorite != null)
            {
                _context.WorkSpaceFavorites.Remove(favorite);
                var result = await _context.SaveChangesAsync();
                return result > 0;
            }
            return false;
        }
    }
}