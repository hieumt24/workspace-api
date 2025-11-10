using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.BookingStatus;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.Application.Features.BookingStatus.Queries;

public record GetAllBookingStatusQuery : IRequest<IEnumerable<BookingStatusDto>>;

public class GetAllBookingStatusQueryHandler : IRequestHandler<GetAllBookingStatusQuery, IEnumerable<BookingStatusDto>>
{
    private readonly IBookingStatusRepository _repository;
    private readonly IMapper _mapper;

    public GetAllBookingStatusQueryHandler(IBookingStatusRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BookingStatusDto>> Handle(GetAllBookingStatusQuery request, CancellationToken cancellationToken)
    {
        var bookingStatuses = await _repository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<BookingStatusDto>>(bookingStatuses);
    }
}

