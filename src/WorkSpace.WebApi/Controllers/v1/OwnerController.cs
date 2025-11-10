using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.Owner.Commands;
using WorkSpace.Application.Features.Owner.Queries;
using WorkSpace.Application.DTOs.Owner;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/owner")]
    [ApiController]
    [Authorize(Roles = nameof(Roles.Owner))]
    public class OwnerController : BaseApiController
    {
        #region Workspace & Room Management

        [HttpGet("workspaces")]
        public async Task<IActionResult> GetMyWorkspaces(CancellationToken ct)
        {
            var userId = User.GetUserId();
            var query = new GetOwnerWorkspacesQuery { OwnerUserId = userId };
            return Ok(await Mediator.Send(query, ct));
        }

        [HttpPost("workspaces")]
        public async Task<IActionResult> CreateWorkSpace([FromBody] CreateWorkSpaceDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var command = new WorkSpace.Application.Features.Owner.Commands.CreateWorkSpaceCommand
            {
                Dto = dto,
                OwnerUserId = userId
            };
            return Ok(await Mediator.Send(command, ct));
        }

        [HttpPost("workspaces/{workspaceId}/rooms")]
        public async Task<IActionResult> CreateWorkSpaceRoom(int workspaceId, [FromBody] CreateWorkSpaceRoomDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var command = new CreateWorkSpaceRoomCommand 
            {
                WorkspaceId = workspaceId,
                Dto = dto,
                OwnerUserId = userId
            };
            return Ok(await Mediator.Send(command, ct));
        }

  
        [HttpPut("rooms/{roomId}")]
        public async Task<IActionResult> UpdateWorkSpaceRoom(int roomId, [FromBody] UpdateWorkSpaceRoomDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var command = new UpdateWorkSpaceRoomCommand
            {
                RoomId = roomId,
                Dto = dto,
                OwnerUserId = userId
            };
            return Ok(await Mediator.Send(command, ct));
        }

        #endregion

        #region Schedule Management


        [HttpPost("rooms/{roomId}/block-slot")]
        public async Task<IActionResult> BlockTimeSlot(int roomId, [FromBody] CreateBlockedTimeSlotDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var command = new CreateBlockedTimeSlotCommand
            {
                RoomId = roomId,
                Dto = dto,
                OwnerUserId = userId
            };
            return Ok(await Mediator.Send(command, ct));
        }

        [HttpDelete("blocked-slots/{slotId}")]
        public async Task<IActionResult> DeleteBlockedTimeSlot(int slotId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var command = new DeleteBlockedTimeSlotCommand { SlotId = slotId, OwnerUserId = userId };
            return Ok(await Mediator.Send(command, ct));
        }

        #endregion

        #region Booking Management


        [HttpGet("bookings")]
        public async Task<IActionResult> GetMyBookings([FromQuery] GetOwnerBookingsQuery query, CancellationToken ct)
        {
            query.OwnerUserId = User.GetUserId();
            return Ok(await Mediator.Send(query, ct));
        }

        [HttpPut("bookings/{bookingId}/confirm")]
        public async Task<IActionResult> ConfirmBooking(int bookingId, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var command = new ManageBookingCommand
            {
                BookingId = bookingId,
                Action = BookingAction.Confirm,
                OwnerUserId = userId
            };
            return Ok(await Mediator.Send(command, ct));
        }

 
        [HttpPut("bookings/{bookingId}/cancel")]
        public async Task<IActionResult> CancelBooking(int bookingId, [FromBody] CancelBookingDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var command = new ManageBookingCommand
            {
                BookingId = bookingId,
                Action = BookingAction.Cancel,
                OwnerUserId = userId,
                Reason = dto.Reason
            };
            return Ok(await Mediator.Send(command, ct));
        }

        #endregion

        #region Performance & Reviews


        [HttpGet("reviews")]
        public async Task<IActionResult> GetMyReviews([FromQuery] GetOwnerReviewsQuery query, CancellationToken ct)
        {
            query.OwnerUserId = User.GetUserId();
            return Ok(await Mediator.Send(query, ct));
        }


        [HttpGet("stats")]
        public async Task<IActionResult> GetMyStats([FromQuery] GetOwnerDashboardQuery query, CancellationToken ct)
        {
            query.OwnerUserId = User.GetUserId();
            return Ok(await Mediator.Send(query, ct));
        }

        #endregion
    }
}