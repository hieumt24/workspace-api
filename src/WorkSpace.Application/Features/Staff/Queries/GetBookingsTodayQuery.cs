using MediatR;
using WorkSpace.Application.DTOs.Bookings;
using System.Collections.Generic;

namespace WorkSpace.Application.Features.Staff.Queries.GetBookingsToday;


public class GetBookingsTodayQuery : IRequest<IEnumerable<BookingAdminDto>>
{
}