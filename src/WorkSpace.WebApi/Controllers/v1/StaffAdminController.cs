using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.DTOs.Refund;
using WorkSpace.Application.DTOs.Support;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.HostProfile.Commands.ApproveHostProfile;
using WorkSpace.Application.Features.HostProfile.Queries.GetAllHostProfiles;
using WorkSpace.Application.Features.Refunds.Commands;
using WorkSpace.Application.Features.Reviews.Commands;
using WorkSpace.Application.Features.Reviews.Queries;
using WorkSpace.Application.Features.Staff.Queries;
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
                [FromQuery] bool? isVerified,
                [FromQuery] bool? isPublic,
                CancellationToken cancellationToken)
    {
        var query = new GetAllReviewsForModerationQuery
        {
            IsVerifiedFilter = isVerified,
            IsPublicFilter = isPublic
        };

        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("reviews/{id}")]
    public async Task<IActionResult> GetReviewDetail([FromRoute] int id, CancellationToken cancellationToken)
    {
        var query = new GetReviewDetailQuery(id);
        var result = await Mediator.Send(query, cancellationToken);

        if (!result.Succeeded)
        {
            return NotFound(new { error = result.Message });
        }


        return Ok(result.Data);
    }

    [HttpPut("reviews/{id}/toggle-visibility")]
    public async Task<IActionResult> ToggleReviewVisibility([FromRoute] int id, CancellationToken cancellationToken)
    {
        var command = new ToggleReviewVisibilityCommand(id);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }
    [HttpPut("reviews/{reviewId}/approve")]
    public async Task<IActionResult> ApproveReview(
         [FromRoute] int reviewId,
         CancellationToken cancellationToken)
    {
   
        var command = new ApproveReviewCommand(reviewId);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }


    //[HttpPut("reviews/{reviewId}/hide")]
    //public async Task<IActionResult> HideReview(
    //    [FromRoute] int reviewId,
    //    CancellationToken cancellationToken)
    //{
    //    var command = new HideReviewCommand(reviewId);
    //    var result = await Mediator.Send(command, cancellationToken);

    //    if (!result.Succeeded) return BadRequest(result);
    //    return Ok(result);
    //}

 
    //[HttpPut("reviews/{reviewId}/show")]
    //public async Task<IActionResult> ShowReview(
    //    [FromRoute] int reviewId,
    //    CancellationToken cancellationToken)
    //{
    //    var command = new ShowReviewCommand(reviewId);
    //    var result = await Mediator.Send(command, cancellationToken);

    //    if (!result.Succeeded) return BadRequest(result);
    //    return Ok(result);
    //}

    //[HttpGet("bookings")]
    //public async Task<IActionResult> GetAllBookings(
    //    [FromQuery] GetAllBookingsQuery query, 
    //    CancellationToken cancellationToken)
    //{
    //    var result = await Mediator.Send(query, cancellationToken);
    //    return Ok(result); 
    //}
    // src/WorkSpace.WebApi/Controllers/v1/StaffAdminController.cs

    [HttpGet("workspaces/pending")]
    public async Task<IActionResult> GetPendingWorkSpaces(
           CancellationToken cancellationToken = default)
    {

        var query = new GetPendingWorkSpacesQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("workspaces")]
    public async Task<IActionResult> GetAllWorkSpaces(
           [FromQuery] bool? isVerified = null,
           CancellationToken cancellationToken = default)
    {
        var query = new GetAllWorkSpacesQuery(IsVerified: isVerified);
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("workspaces/{workSpaceId}/approve")]
    public async Task<IActionResult> ApproveWorkSpace(
             [FromRoute] int workSpaceId,
             CancellationToken cancellationToken = default)
    {

        var dto = new Application.DTOs.WorkSpaces.ApproveWorkSpaceDto
        {
            WorkSpaceId = workSpaceId,
            IsApproved = true,
            RejectionReason = null
        };

        var command = new ApproveWorkSpaceCommand(dto);
        var result = await Mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(new { error = result.Message });
        }

        return Ok(result.Data);
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
        if (staffUserId == 0) return Unauthorized(new { error = "Invalid user token" });

        var command = new RequestRefundCommand
        {
            BookingId = bookingId,
            StaffUserId = staffUserId,
            Notes = dto.Notes
        };

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.Succeeded) return BadRequest(new { error = result.Message });

      
        return Ok(result.Data);
    }

    [HttpPost("refund-requests/{refundRequestId}/process")]
    public async Task<IActionResult> ProcessRefund(
         [FromRoute] int refundRequestId,
         CancellationToken cancellationToken)
    {
        var staffUserId = User.GetUserId();
        if (staffUserId == 0) return Unauthorized(new { error = "Invalid user token" });

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";

        var command = new ProcessRefundCommand
        {
            RefundRequestId = refundRequestId,
            StaffUserId = staffUserId,
            IpAddress = ipAddress
        };

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.Succeeded) return BadRequest(new { error = result.Message });

      
        return Ok(new { transactionId = result.Data });
    }
    [HttpGet("owner-registrations")]
    public async Task<IActionResult> GetOwnerRegistrations(
            CancellationToken cancellationToken = default)
    {

        var query = new GetAllHostProfilesQuery
        {
            IsVerified = false,
            PageNumber = 1,
            PageSize = int.MaxValue
        };

        var result = await Mediator.Send(query, cancellationToken);

        return Ok(result.Data);
    }

    [HttpPut("owner-registrations/{hostProfileId}/approve")]
    public async Task<IActionResult> ApproveOwnerRegistration(
         [FromRoute] int hostProfileId,
         [FromQuery] bool isApproved = true, 
         CancellationToken cancellationToken = default)
    {
        var command = new ApproveHostProfileCommand
        {
            HostProfileId = hostProfileId,
            IsApproved = isApproved
        };

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            
            return BadRequest(new { error = result.Message });
        }

     
        return Ok(new { success = true, isVerified = isApproved });
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
        if (staffUserId == 0) return Unauthorized(new { error = "Invalid user token" });

        var command = new StaffReplyToTicketCommand
        {
            TicketId = ticketId,
            Message = request.Message,
            StaffUserId = staffUserId
        };

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.Succeeded) return BadRequest(new { error = result.Message });

        return Ok(result.Data);
    }


    [HttpPut("support-tickets/{ticketId}/status")]
    public async Task<IActionResult> UpdateTicketStatus(
         [FromRoute] int ticketId,
         [FromBody] UpdateTicketStatusRequestDto request,
         CancellationToken cancellationToken)
    {
        var staffUserId = User.GetUserId();
        if (staffUserId == 0) return Unauthorized(new { error = "Invalid user token" });

        var command = new UpdateTicketStatusCommand
        {
            TicketId = ticketId,
            NewStatus = request.Status,
            StaffUserId = staffUserId
        };

        var result = await Mediator.Send(command, cancellationToken);

        if (!result.Succeeded) return BadRequest(new { error = result.Message });

    
        return Ok(result.Data);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetStaffDashboard(CancellationToken cancellationToken)
    {
        var query = new GetStaffDashboardQuery();
        var result = await Mediator.Send(query, cancellationToken);
        return Ok(result);
    }


    [HttpGet("bookings/today")]
    public async Task<IActionResult> GetBookingsToday(CancellationToken cancellationToken)
    {
        var query = new WorkSpace.Application.Features.Staff.Queries.GetBookingsToday.GetBookingsTodayQuery();
        var result = await Mediator.Send(query, cancellationToken);

  
        return Ok(result);
    }
}