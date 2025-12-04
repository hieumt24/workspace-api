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
   
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetAllBookingStatusQuery(), cancellationToken);
        return Ok(result);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetBookingStatusByIdQuery(id), cancellationToken);
        return Ok(result);
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingStatusCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBookingStatusCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

 
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new DeleteBookingStatusCommand(id), cancellationToken);
        return Ok(result);
    }
}

