using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.BookingStatus.Commands.CreateBookingStatus;

public class CreateBookingStatusCommand : IRequest<Response<int>>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}

public class CreateBookingStatusCommandHandler : IRequestHandler<CreateBookingStatusCommand, Response<int>>
{
    private readonly IBookingStatusRepository _repository;
    private readonly IMapper _mapper;
    private readonly IDateTimeService _dateTimeService;

    public CreateBookingStatusCommandHandler(
        IBookingStatusRepository repository,
        IMapper mapper,
        IDateTimeService dateTimeService)
    {
        _repository = repository;
        _mapper = mapper;
        _dateTimeService = dateTimeService;
    }

    public async Task<Response<int>> Handle(CreateBookingStatusCommand request, CancellationToken cancellationToken)
    {
        // Check if name already exists
        var isUnique = await _repository.IsNameUniqueAsync(request.Name, null, cancellationToken);
        if (!isUnique)
        {
            return new Response<int>($"Booking status with name '{request.Name}' already exists.");
        }

        var bookingStatus = new Domain.Entities.BookingStatus
        {
            Name = request.Name,
            Description = request.Description,
            CreateUtc = _dateTimeService.NowUtc
        };

        try
        {
            await _repository.AddAsync(bookingStatus, cancellationToken);
            return new Response<int>(bookingStatus.Id, "Booking status created successfully.");
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return new Response<int>($"Failed to create booking status. {msg}");
        }
    }
}

