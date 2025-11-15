using MediatR;
using WorkSpace.Application.DTOs.Recommendations;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Recommendations.Queries.GetUserPreferences;

public record GetUserPreferencesQuery(int UserId) : IRequest<Response<UserPreferenceDto>>;