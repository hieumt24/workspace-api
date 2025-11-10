using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.Bookings.Commands;
using WorkSpace.Application.Features.Bookings.Queries;

namespace WorkSpace.WebApi.Controllers.v1;
[Route("api/v1/bookings")]
public class BookingController : BaseApiController
{
    /// <summary>
    /// Create a new booking
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId == 0)
        {
            return Unauthorized("Invalid user token");
        }

        // Override customerId from token for security
        request.CustomerId = userId;

        var command = new CreateBookingCommand 
        { 
            Model = request 
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get all bookings for the current user
    /// </summary>
    [HttpGet("my-bookings")]
    [Authorize]
    public async Task<IActionResult> GetMyBookings(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? statusIdFilter = null,
        [FromQuery] DateTimeOffset? startDateFilter = null,
        [FromQuery] DateTimeOffset? endDateFilter = null,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        if (userId == 0)
        {
            return Unauthorized("Invalid user token");
        }

        var query = new GetUserBookingsQuery
        {
            UserId = userId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            StatusIdFilter = statusIdFilter,
            StartDateFilter = startDateFilter,
            EndDateFilter = endDateFilter
        };

        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}