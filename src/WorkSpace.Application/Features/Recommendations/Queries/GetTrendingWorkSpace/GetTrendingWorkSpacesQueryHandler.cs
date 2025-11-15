using MediatR;
using WorkSpace.Application.DTOs.Recommendations;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Recommendations.Queries.GetTrendingWorkSpace;

public class GetTrendingWorkSpacesQueryHandler : IRequestHandler<
    GetTrendingWorkSpacesQuery,
    Response<List<RecommendedWorkSpaceDto>>
    >
{
    private readonly IRecommendationService _recommendationService;
    public GetTrendingWorkSpacesQueryHandler(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }
    public async Task<Response<List<RecommendedWorkSpaceDto>>> Handle(GetTrendingWorkSpacesQuery request, CancellationToken cancellationToken)
    {
        var trendingWorkspaces = await _recommendationService.GetTrendingWorkSpacesAsync(request.Count, cancellationToken);
        return new Response<List<RecommendedWorkSpaceDto>>(
            trendingWorkspaces, 
            $"Retrieved {trendingWorkspaces.Count} trending workspaces");
    }
}