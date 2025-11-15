using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Recommendations;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.Recommendations.Queries.GetPersonalizedRecommendation;
using WorkSpace.Application.Features.Recommendations.Queries.GetTrendingWorkSpace;
using WorkSpace.Application.Features.Recommendations.Queries.GetUserPreferences;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("api/v1/recommendations")]
[ApiController]
public class RecommendationsController : BaseApiController
{
    /// <summary>
    /// Lấy các workspace được đề xuất cá nhân hóa dựa trên lịch sử booking của user
    /// </summary>
    [HttpPost("personalized")]
    [Authorize]
    public async Task<IActionResult> GetPersonalizedRecommendations(
        [FromBody] GetRecommendationsRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == 0)
        {
            return Unauthorized("Invalid user token");
        }
    
        // Override userId from token for security
        request.UserId = userId;
    
        var query = new GetPersonalizedRecommendationsQuery(request);
        var result = await Mediator.Send(query, cancellationToken);
    
        return Ok(result);
    }

    /// <summary>
    /// Lấy danh sách trending workspaces (hot nhất trong 30 ngày qua)
    /// </summary>
    [HttpGet("trending")]
    public async Task<IActionResult> GetTrendingWorkSpaces(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTrendingWorkSpacesQuery(count);
        var result = await Mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin phân tích preferences của user (cho debugging/admin)
    /// </summary>
    [HttpGet("my-preferences")]
    [Authorize]
    public async Task<IActionResult> GetMyPreferences(
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == 0)
        {
            return Unauthorized("Invalid user token");
        }

        var query = new GetUserPreferencesQuery(userId);
        var result = await Mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Admin: Lấy preferences của bất kỳ user nào
    /// </summary>
    [HttpGet("users/{userId}/preferences")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetUserPreferences(
        [FromRoute] int userId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserPreferencesQuery(userId);
        var result = await Mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}