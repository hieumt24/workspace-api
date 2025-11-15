using MediatR;
using WorkSpace.Application.DTOs.Recommendations;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Recommendations.Queries.GetPersonalizedRecommendation;

public record GetPersonalizedRecommendationsQuery : IRequest<PagedResponse<List<RecommendedWorkSpaceDto>>>
{
    public GetRecommendationsRequestDto Request { get; init; }

    public GetPersonalizedRecommendationsQuery(GetRecommendationsRequestDto request)
    {
        Request = request;
    }
}