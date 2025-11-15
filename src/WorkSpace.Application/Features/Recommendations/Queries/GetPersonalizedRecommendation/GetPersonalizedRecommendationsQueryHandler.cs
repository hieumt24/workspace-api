using MediatR;
using WorkSpace.Application.DTOs.Recommendations;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Recommendations.Queries.GetPersonalizedRecommendation;

public class GetPersonalizedRecommendationsQueryHandler 
    : IRequestHandler<GetPersonalizedRecommendationsQuery, PagedResponse<List<RecommendedWorkSpaceDto>>>
{
    private readonly IRecommendationService _recommendationService;

    public GetPersonalizedRecommendationsQueryHandler(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    public async Task<PagedResponse<List<RecommendedWorkSpaceDto>>> Handle(
        GetPersonalizedRecommendationsQuery query, 
        CancellationToken cancellationToken)
    {
        // Gọi service để lấy recommendations
        var (recommendations, totalCount) = await _recommendationService
            .GetPersonalizedRecommendationsAsync(query.Request, cancellationToken);

        // Tạo paged response
        return new PagedResponse<List<RecommendedWorkSpaceDto>>(
            data: recommendations, 
            pageNumber: query.Request.PageNumber, 
            pageSize: query.Request.PageSize,
            totalRecords: totalCount)
        {
            Message = recommendations.Any() 
                ? $"Found {recommendations.Count} personalized recommendations"
                : "No recommendations found. Try adjusting your filters or book more workspaces to improve recommendations.",
            Succeeded = true
        };
    }
}