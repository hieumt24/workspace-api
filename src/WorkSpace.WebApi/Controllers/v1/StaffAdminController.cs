using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.DTOs.Refund;
using WorkSpace.Application.DTOs.Support;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.Refunds.Commands;
using WorkSpace.Application.Features.Reviews.Commands;
using WorkSpace.Application.Features.Reviews.Queries;
using WorkSpace.Application.Features.SupportTickets.Commands;
using WorkSpace.Application.Features.SupportTickets.Queries;
using WorkSpace.Application.Features.WorkSpace.Commands;
using WorkSpace.Application.Features.WorkSpace.Queries;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/staff")]

[Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Staff)}")]

[ApiController]
public class StaffAdminController : BaseApiController
{
    [HttpGet("reviews")]
    public async Task<IActionResult> GetAllReviewsForModeration(
        [FromQuery] GetAllReviewsForModerationQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("reviews/{reviewId}/moderate")]
    public async Task<IActionResult> ModerateReview(
        [FromRoute] int reviewId,
        [FromBody] ModerateReviewCommand command,
        CancellationToken cancellationToken)
    {
        
        command.ReviewId = reviewId;

        var result = await Mediator.Send(command, cancellationToken);
   
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }


    //[HttpGet("bookings")]
    //public async Task<IActionResult> GetAllBookings(
    //    [FromQuery] GetAllBookingsQuery query, 
    //    CancellationToken cancellationToken)
    //{
    //    var result = await Mediator.Send(query, cancellationToken);
    //    return Ok(result); 
    //}


    [HttpGet("workspaces/pending")]
    public async Task<IActionResult> GetPendingWorkSpaces(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPendingWorkSpacesQuery(pageNumber, pageSize);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }


    [HttpGet("workspaces")]
    public async Task<IActionResult> GetAllWorkSpaces(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isVerified = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllWorkSpacesQuery(pageNumber, pageSize, isVerified);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }


    [HttpPut("workspaces/{workSpaceId}/approve")]
    public async Task<IActionResult> ApproveWorkSpace(
        [FromRoute] int workSpaceId,
        [FromBody] Application.DTOs.WorkSpaces.ApproveWorkSpaceDto dto,
        CancellationToken cancellationToken = default)
    {
        dto.WorkSpaceId = workSpaceId;
        var command = new ApproveWorkSpaceCommand(dto);
        var result = await Mediator.Send(command, cancellationToken);
        
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        
        return Ok(result);
    }
    //[HttpPost("bookings/{bookingId}/cancel")]
    //public async Task<IActionResult> StaffCancelBooking(
    //    [FromRoute] int bookingId,
    //    [FromBody] StaffCancelBookingRequestDto request,
    //    CancellationToken cancellationToken)
    //{
    //    var staffUserId = User.GetUserId();
    //    if (staffUserId == 0) return Unauthorized();

    //    var command = new StaffCancelBookingCommand
    //    {
    //        BookingId = bookingId,
    //        Reason = request.Reason,
    //        StaffUserId = staffUserId
    //    };

    //    var result = await Mediator.Send(command, cancellationToken);
    //    return Ok(result);
    //}


    //[HttpPut("bookings/{bookingId}/reschedule")]
    //public async Task<IActionResult> StaffRescheduleBooking(
    //    [FromRoute] int bookingId,
    //    [FromBody] StaffRescheduleBookingRequestDto request,
    //    CancellationToken cancellationToken)
    //{
    //    var staffUserId = User.GetUserId();
    //    if (staffUserId == 0) return Unauthorized();

    //    var command = new StaffRescheduleBookingCommand
    //    {
    //        BookingId = bookingId,
    //        NewStartTimeUtc = request.NewStartTimeUtc,
    //        NewEndTimeUtc = request.NewEndTimeUtc,
    //        StaffUserId = staffUserId
    //    };

    //    var result = await Mediator.Send(command, cancellationToken);
    //    return Ok(result);
    //}

    //[HttpPost("bookings/{bookingId}/confirm-payment")]
    //public async Task<IActionResult> StaffConfirmPayment(
    //    [FromRoute] int bookingId,
    //    [FromBody] StaffConfirmPaymentRequestDto request,
    //    CancellationToken cancellationToken)
    //{
    //    var staffUserId = User.GetUserId();
    //    if (staffUserId == 0) return Unauthorized();

    //    var command = new StaffConfirmPaymentCommand
    //    {
    //        BookingId = bookingId,
    //        PaymentMethod = request.PaymentMethod,
    //        TransactionId = request.TransactionId,
    //        Amount = request.Amount,
    //        StaffUserId = staffUserId
    //    };

    //    var result = await Mediator.Send(command, cancellationToken);
    //    return Ok(result);
    //}
    [HttpPost("bookings/{bookingId}/refund/request")]
    public async Task<IActionResult> RequestRefund(
        [FromRoute] int bookingId,
        [FromBody] CreateRefundRequestDto dto,
        CancellationToken cancellationToken)
    {
        var staffUserId = User.GetUserId();
        if (staffUserId == 0) return Unauthorized(new Response<string>("Invalid user token"));

        var command = new RequestRefundCommand
        {
            BookingId = bookingId,
            StaffUserId = staffUserId,
            Notes = dto.Notes
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("refund-requests/{refundRequestId}/process")]
    public async Task<IActionResult> ProcessRefund(
        [FromRoute] int refundRequestId,
        CancellationToken cancellationToken)
    {
        var staffUserId = User.GetUserId();
        if (staffUserId == 0) return Unauthorized(new Response<string>("Invalid user token"));

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";

        var command = new ProcessRefundCommand
        {
            RefundRequestId = refundRequestId,
            StaffUserId = staffUserId,
            IpAddress = ipAddress
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet("support-tickets")]
    public async Task<IActionResult> GetSupportTickets(
        [FromQuery] GetSupportTicketsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }


    [HttpPost("support-tickets/{ticketId}/reply")]
    public async Task<IActionResult> ReplyToTicket(
        [FromRoute] int ticketId,
        [FromBody] StaffReplyRequest request,
        CancellationToken cancellationToken)
    {
        var staffUserId = User.GetUserId();
        if (staffUserId == 0) return Unauthorized(new Response<string>("Invalid user token"));

        var command = new StaffReplyToTicketCommand
        {
            TicketId = ticketId,
            Message = request.Message,
            StaffUserId = staffUserId
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPut("support-tickets/{ticketId}/status")]
    public async Task<IActionResult> UpdateTicketStatus(
        [FromRoute] int ticketId,
        [FromBody] UpdateTicketStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var staffUserId = User.GetUserId();
        if (staffUserId == 0) return Unauthorized(new Response<string>("Invalid user token"));

        var command = new UpdateTicketStatusCommand
        {
            TicketId = ticketId,
            NewStatus = request.Status,
            StaffUserId = staffUserId
        };

        var result = await Mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}