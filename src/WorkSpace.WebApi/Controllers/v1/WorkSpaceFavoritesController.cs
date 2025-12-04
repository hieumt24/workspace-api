using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.WorkSpaces;
using System.Threading.Tasks;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Authorize]
    [Route("api/v1/workspacefavorite")]
    [ApiController]
    public class WorkSpaceFavoritesController : ControllerBase
    {
        private readonly IWorkSpaceFavoriteRepository _workSpaceFavoriteRepository;

        public WorkSpaceFavoritesController(IWorkSpaceFavoriteRepository workSpaceFavoriteRepository)
        {
            _workSpaceFavoriteRepository = workSpaceFavoriteRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddToFavorites(int workSpaceId)
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated");
            }
            var result = await _workSpaceFavoriteRepository.AddToFavoritesAsync(workSpaceId, userId);
            if (result)
                return Ok(new { message = "Workspace added to favorites." });
            return BadRequest(new { message = "Failed to add workspace to favorites." });
        }

        [HttpDelete("{workSpaceId}")]
        public async Task<IActionResult> RemoveFromFavorites(int workSpaceId)
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated");
            }
            var result = await _workSpaceFavoriteRepository.RemoveFromFavoritesAsync(workSpaceId, userId);
            if (result)
                return Ok(new { message = "Workspace removed from favorites." });
            return BadRequest(new { message = "Failed to remove workspace from favorites." });
        }

        [HttpGet("isfavorite/{workSpaceId}")]
        public async Task<IActionResult> IsFavorite(int workSpaceId)
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated");
            }
            var isFavorite = await _workSpaceFavoriteRepository.IsFavoriteAsync(workSpaceId, userId);
            return Ok(new { isFavorite });
        }

        [HttpGet("userfavorites")]
        public async Task<IActionResult> GetFavoriteWorkSpaces()
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated");
            }
            var favoriteWorkSpaces = await _workSpaceFavoriteRepository.GetFavoriteWorkSpacesAsync(userId);
            return Ok(favoriteWorkSpaces);
        }


        [HttpGet("userfavoriteids")]
        public async Task<IActionResult> GetFavoriteWorkSpaceIds()
        {
            var userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized("User not authenticated");
            }
            var favoriteWorkSpaceIds = await _workSpaceFavoriteRepository.GetFavoriteWorkSpaceIdsAsync(userId);
            return Ok(favoriteWorkSpaceIds);
        }

    }
}