

using AutoMapper;
using MediatR;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Application.Mappings;

namespace WorkSpace.Application.Features.HostProfile.Queries.GetHostProfileByUserId;

public class GetHostProfileByUserIdQuery : IRequest<Response<HostProfileDto>>
{
    public int UserId { get; set; }

    public GetHostProfileByUserIdQuery(int userId)
    {
        UserId = userId;
    }
}

public class GetHostProfileByUserIdQueryHandler : IRequestHandler<GetHostProfileByUserIdQuery, Response<HostProfileDto>>
{
    private readonly IHostProfileAsyncRepository _hostProfileRepository;
    private readonly IMapper _mapper;

    public GetHostProfileByUserIdQueryHandler(IHostProfileAsyncRepository hostProfileRepository, IMapper mapper)
    {
        _hostProfileRepository = hostProfileRepository;
        _mapper = mapper;
    }

    public async Task<Response<HostProfileDto>> Handle(GetHostProfileByUserIdQuery request, CancellationToken cancellationToken)
    {
       
        var hostProfile = await _hostProfileRepository.GetHostProfileByUserId(request.UserId, cancellationToken);

        if (hostProfile == null)
        {
            return new Response<HostProfileDto>($"Không tìm thấy hồ sơ Host cho User ID {request.UserId}.");
        }

     
        var hostProfileDto = _mapper.Map<HostProfileDto>(hostProfile);

        return new Response<HostProfileDto>(hostProfileDto);
    }
}