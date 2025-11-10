using AutoMapper;
using MediatR;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Mappings;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.HostProfile.Queries.GetHostProfileById;

public class GetHostProfileByIdQuery : IRequest<Response<HostProfileDto>>
{
    public int Id { get; set; }
    
    public GetHostProfileByIdQuery(int id)
    {
        Id = id;
    }
}

public class GetHostProfileByIdQueryHandler : IRequestHandler<GetHostProfileByIdQuery, Response<HostProfileDto>>
{
    private readonly IHostProfileAsyncRepository _hostProfileRepository;
    private readonly IMapper _mapper;

    public GetHostProfileByIdQueryHandler(IHostProfileAsyncRepository hostProfileRepository, IMapper mapper)
    {
        _hostProfileRepository = hostProfileRepository;
        _mapper = mapper;
    }

    public async Task<Response<HostProfileDto>> Handle(GetHostProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var hostProfile = await _hostProfileRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (hostProfile == null)
        {
            return new Response<HostProfileDto>($"Host profile with ID {request.Id} not found.");
        }

        var hostProfileDto = _mapper.Map<HostProfileDto>(hostProfile);
        return new Response<HostProfileDto>(hostProfileDto);
    }
}
