using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.BookingStatus;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.BookingStatus.Queries;

public record GetBookingStatusByIdQuery(int Id) : IRequest<Response<BookingStatusDto>>;

public class GetBookingStatusByIdQueryHandler : IRequestHandler<GetBookingStatusByIdQuery, Response<BookingStatusDto>>
{
    private readonly IBookingStatusRepository _repository;
    private readonly IMapper _mapper;

    public GetBookingStatusByIdQueryHandler(IBookingStatusRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Response<BookingStatusDto>> Handle(GetBookingStatusByIdQuery request, CancellationToken cancellationToken)
    {
        var bookingStatus = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (bookingStatus == null)
        {
            return new Response<BookingStatusDto>($"Booking status with id {request.Id} not found.");
        }

        var dto = _mapper.Map<BookingStatusDto>(bookingStatus);
        return new Response<BookingStatusDto>(dto);
    }
}

