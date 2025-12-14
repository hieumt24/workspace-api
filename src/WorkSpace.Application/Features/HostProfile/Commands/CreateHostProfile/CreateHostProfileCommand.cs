using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using System.Text.Json.Serialization;
using WorkSpace.Domain.Entities; 

namespace WorkSpace.Application.Features.HostProfile.Commands.CreateHostProfile;

public partial class CreateHostProfileCommand : IRequest<Response<int>>
{
    [JsonIgnore]
    public int UserId { get; set; }

    public string? CompanyName { get; set; }

    public string? Description { get; set; }

    public string? ContactPhone { get; set; }

    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }

    public string? Avatar { get; set; }
    public string? CoverPhoto { get; set; }

    public List<string>? DocumentUrls { get; set; } = new();
}

public class CreateHostProfileCommandHandler : IRequestHandler<CreateHostProfileCommand, Response<int>>
{
    private readonly IHostProfileAsyncRepository _hostProfileRepository;
    private readonly IMapper _mapper;
    private readonly IDateTimeService _dateTimeService;

    public CreateHostProfileCommandHandler(
        IHostProfileAsyncRepository hostProfileRepository,
        IMapper mapper,
        IDateTimeService dateTimeService)
    {
        _hostProfileRepository = hostProfileRepository;
        _mapper = mapper;
        _dateTimeService = dateTimeService;
    }

    public async Task<Response<int>> Handle(CreateHostProfileCommand request, CancellationToken cancellationToken)
    {
     
        var existingProfile = await _hostProfileRepository.GetHostProfileByUserId(request.UserId, cancellationToken);
        if (existingProfile != null)
        {
            return new Response<int>($"User with id {request.UserId} already has a host profile.");
        }

        var hostProfile = _mapper.Map<Domain.Entities.HostProfile>(request);

        hostProfile.CreateUtc = _dateTimeService.NowUtc;
        hostProfile.IsVerified = false;

  
        if (request.DocumentUrls != null && request.DocumentUrls.Any())
        {
            foreach (var url in request.DocumentUrls)
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    hostProfile.Documents.Add(new HostProfileDocument
                    {
                        FileUrl = url,
                     
                    });
                }
            }
        }

        try
        {
           
            await _hostProfileRepository.AddAsync(hostProfile, cancellationToken);
            return new Response<int>(hostProfile.Id, "Host profile created successfully.");
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return new Response<int>($"Save Host profile failed. {msg}");
        }
    }
}