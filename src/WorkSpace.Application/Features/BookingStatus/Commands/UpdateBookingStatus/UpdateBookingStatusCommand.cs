using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.BookingStatus.Commands.UpdateBookingStatus;

public class UpdateBookingStatusCommand : IRequest<Response<int>>
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class UpdateBookingStatusCommandHandler : IRequestHandler<UpdateBookingStatusCommand, Response<int>>
{
    private readonly IBookingStatusRepository _repository;
    private readonly IDateTimeService _dateTimeService;

    public UpdateBookingStatusCommandHandler(
        IBookingStatusRepository repository,
        IDateTimeService dateTimeService)
    {
        _repository = repository;
        _dateTimeService = dateTimeService;
    }

    public async Task<Response<int>> Handle(UpdateBookingStatusCommand request, CancellationToken cancellationToken)
    {
        var bookingStatus = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (bookingStatus == null)
        {
            return new Response<int>($"Booking status with id {request.Id} not found.");
        }

        // Check if new name already exists (excluding current record)
        var isUnique = await _repository.IsNameUniqueAsync(request.Name, request.Id, cancellationToken);
        if (!isUnique)
        {
            return new Response<int>($"Booking status with name '{request.Name}' already exists.");
        }

        bookingStatus.Name = request.Name;
        bookingStatus.Description = request.Description;
        bookingStatus.LastModifiedUtc = _dateTimeService.NowUtc;

        try
        {
            await _repository.UpdateAsync(bookingStatus, cancellationToken);
            return new Response<int>(bookingStatus.Id, "Booking status updated successfully.");
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return new Response<int>($"Failed to update booking status. {msg}");
        }
    }
}

