using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.BookingStatus.Commands.DeleteBookingStatus;

public record DeleteBookingStatusCommand(int Id) : IRequest<Response<int>>;

public class DeleteBookingStatusCommandHandler : IRequestHandler<DeleteBookingStatusCommand, Response<int>>
{
    private readonly IBookingStatusRepository _repository;

    public DeleteBookingStatusCommandHandler(IBookingStatusRepository repository)
    {
        _repository = repository;
    }

    public async Task<Response<int>> Handle(DeleteBookingStatusCommand request, CancellationToken cancellationToken)
    {
        var bookingStatus = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (bookingStatus == null)
        {
            return new Response<int>($"Booking status with id {request.Id} not found.");
        }


        if (bookingStatus.Bookings != null && bookingStatus.Bookings.Any())
        {
            return new Response<int>($"Cannot delete booking status. There are {bookingStatus.Bookings.Count} booking(s) using this status.");
        }

        try
        {
            await _repository.DeleteAsync(bookingStatus, cancellationToken);
            return new Response<int>(bookingStatus.Id, "Booking status deleted successfully.");
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return new Response<int>($"Failed to delete booking status. {msg}");
        }
    }
}

