using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Owner;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Features.Owner.Commands;
using WorkSpace.Application.Features.Owner.Queries;
using WorkSpace.Application.Features.Refunds.Commands;

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

            var result = await Mediator.Send(query, ct);
            return Ok(result.Data);
        }

        [HttpPut("workspaces/{id}")]
        public async Task<IActionResult> UpdateWorkSpace(int id, [FromBody] UpdateWorkSpaceDto dto, CancellationToken ct)
        {
            var userId = User.GetUserId();
            var command = new UpdateWorkSpaceCommand
            {
                WorkSpaceId = id,
                Dto = dto,
                OwnerUserId = userId
            };

            return Ok(await Mediator.Send(command, ct));
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
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] int? statusIdFilter,
            [FromQuery] int? workSpaceIdFilter,
            CancellationToken ct)
        {
            var query = new GetOwnerBookingsQuery
            {
                OwnerUserId = User.GetUserId(),
                StatusIdFilter = statusIdFilter,
                WorkSpaceIdFilter = workSpaceIdFilter
            };

            var result = await Mediator.Send(query, ct);

            return Ok(result.Data);
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

        [HttpPost("refund-requests/{refundRequestId}/approve")]
        public async Task<IActionResult> ApproveOrRejectRefund(
            [FromRoute] int refundRequestId,
            [FromBody] ApproveRefundCommand command,
            CancellationToken ct)
        {
            var userId = User.GetUserId();

            command.RefundRequestId = refundRequestId;
            command.OwnerUserId = userId;

            return Ok(await Mediator.Send(command, ct));
        }
        #endregion

        #region Performance & Reviews


        [HttpGet("workspaces/{workspaceId}/reviews")]
        public async Task<IActionResult> GetReviewsByWorkspace(
            int workspaceId,
            CancellationToken ct = default)
        {
            var userId = User.GetUserId();

            var query = new GetOwnerReviewsQuery
            {
                OwnerUserId = userId,
                WorkSpaceIdFilter = workspaceId
            };

            var result = await Mediator.Send(query, ct);

            return Ok(result.Data);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetMyStats(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            CancellationToken ct)
        {
            var query = new GetOwnerDashboardQuery
            {
                OwnerUserId = User.GetUserId(),
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await Mediator.Send(query, ct);

            return Ok(result.Data);
        }

        #endregion

        [HttpGet("workspaces/{workspaceId}/rooms")]
        public async Task<IActionResult> GetRoomsInWorkspace(
            int workspaceId,
            CancellationToken ct = default)
        {
            var userId = User.GetUserId();

            var query = new WorkSpace.Application.Features.Owner.Queries.GetOwnerWorkspaceRoomsQuery
            {
                OwnerUserId = userId,
                WorkspaceId = workspaceId
            };

            var result = await Mediator.Send(query, ct);

            return Ok(result);
        }
    }
}