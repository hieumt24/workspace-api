using MediatR;
using WorkSpace.Application.DTOs.Recommendations;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Recommendations.Queries.GetTrendingWorkSpace;

public record GetTrendingWorkSpacesQuery(int Count = 10) : IRequest<Response<List<RecommendedWorkSpaceDto>>>;

