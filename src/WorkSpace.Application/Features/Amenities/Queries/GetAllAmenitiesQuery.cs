using AutoMapper;
using MediatR;
using WorkSpace.Application.DTOs.Amenities;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Amenities.Queries;

public record GetAllAmenitiesQuery : IRequest<IEnumerable<AmenityDto>>;

public class GetAllAmenitiesQueryHandler(
    IGenericRepositoryAsync<Amenity> repository,
    IMapper mapper) : IRequestHandler<GetAllAmenitiesQuery, IEnumerable<AmenityDto>>
{
    public async Task<IEnumerable<AmenityDto>> Handle(GetAllAmenitiesQuery request, CancellationToken cancellationToken)
    {
        var amenities = await repository.GetAllAsync(cancellationToken);
        return mapper.Map<IEnumerable<AmenityDto>>(amenities);
    }
}
