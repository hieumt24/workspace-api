using WorkSpace.Application.DTOs.Recommendations;

namespace WorkSpace.Application.Interfaces.Services;

public interface IRecommendationService
{
    Task<UserPreferenceDto> AnalyzeUserPreferencesAsync(
        int userId, 
        CancellationToken cancellationToken = default);
    Task<(List<RecommendedWorkSpaceDto> Recommendations, int TotalCount)> GetPersonalizedRecommendationsAsync(
        GetRecommendationsRequestDto request,
        CancellationToken cancellationToken = default);
    Task<List<RecommendedWorkSpaceDto>> GetTrendingWorkSpacesAsync(
        int count = 10,
        CancellationToken cancellationToken = default);
    Task<double> CalculateRecommendationScoreAsync(
        int workspaceId,
        UserPreferenceDto userPreference,
        CancellationToken cancellationToken = default);
}