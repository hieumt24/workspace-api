using MediatR;
using WorkSpace.Application.DTOs.Recommendations;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Recommendations.Queries.GetUserPreferences;

public class GetUserPreferencesQueryHandler : IRequestHandler<GetUserPreferencesQuery, Response<UserPreferenceDto>>
{
    private readonly IRecommendationService _recommendationService;
    public GetUserPreferencesQueryHandler(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }
    public async Task<Response<UserPreferenceDto>> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var preferences = await _recommendationService.AnalyzeUserPreferencesAsync(request.UserId, cancellationToken);
        return new Response<UserPreferenceDto>(
            preferences, 
            $"Analyzed {preferences.TotalBookings} bookings to determine user preferences");
    }
}