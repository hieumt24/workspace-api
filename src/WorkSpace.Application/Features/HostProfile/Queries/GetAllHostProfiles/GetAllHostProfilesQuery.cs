using AutoMapper;
using MediatR;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Mappings;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.HostProfile.Queries.GetAllHostProfiles;

public class GetAllHostProfilesQuery : IRequest<Response<IEnumerable<HostProfileDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool? IsVerified { get; set; }
    public string? CompanyName { get; set; }
    public string? City { get; set; }
}

public class GetAllHostProfilesQueryHandler : IRequestHandler<GetAllHostProfilesQuery, Response<IEnumerable<HostProfileDto>>>
{
    private readonly IHostProfileAsyncRepository _hostProfileRepository;
    private readonly IMapper _mapper;

    public GetAllHostProfilesQueryHandler(IHostProfileAsyncRepository hostProfileRepository, IMapper mapper)
    {
        _hostProfileRepository = hostProfileRepository;
        _mapper = mapper;
    }

    public async Task<Response<IEnumerable<HostProfileDto>>> Handle(GetAllHostProfilesQuery request, CancellationToken cancellationToken)
    {
        var hostProfiles = await _hostProfileRepository.GetAllHostProfilesAsync(
            request.PageNumber, 
            request.PageSize, 
            request.IsVerified, 
            request.CompanyName, 
            request.City, 
            cancellationToken);

        var hostProfileDtos = _mapper.Map<IEnumerable<HostProfileDto>>(hostProfiles);
        return new Response<IEnumerable<HostProfileDto>>(hostProfileDtos);
    }
}
