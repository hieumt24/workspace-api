using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Wrappers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkSpace.Application.Interfaces.Services
{
    public interface ISearchService
    {
        Task<Response<IEnumerable<WorkSpaceSearchResultDto>>> SearchWorkSpacesAsync(SearchRequestDto request);
        Task<IEnumerable<string>> GetLocationSuggestionsAsync(string query);
        Task<IEnumerable<string>> GetAllWardsAsync();
        Task<Response<IEnumerable<RoomWithAmenitiesDto>>> SearchRoomsInWorkSpaceAsync(int workSpaceId, SearchRoomsInWorkSpaceRequestDto request);
    }
}