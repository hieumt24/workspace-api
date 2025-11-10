using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Features.BookingStatus.Commands.CreateBookingStatus;
using WorkSpace.Application.Features.BookingStatus.Commands.UpdateBookingStatus;
using WorkSpace.Application.Features.BookingStatus.Commands.DeleteBookingStatus;
using WorkSpace.Application.Features.BookingStatus.Queries;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/booking-status")]
[ApiController]
public class BookingStatusController : BaseApiController
{
    /// <summary>
    /// Get all booking statuses
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAllBookingStatusQuery(), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get booking status by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetBookingStatusByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new booking status
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingStatusCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update booking status
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookingStatusCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete booking status
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteBookingStatusCommand(id), cancellationToken);
        return Ok(result);
    }
}

