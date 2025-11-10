using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using FluentValidation;

namespace WorkSpace.Application.Features.HostProfile.Commands.UpdateHostProfile;

public class UpdateHostProfileCommand : IRequest<Response<int>>
{
    public int Id { get; set; }
    public string? CompanyName { get; set; }
    public string? Description { get; set; }
    public string? ContactPhone { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool? IsVerified { get; set; }
}

public class UpdateHostProfileCommandHandler : IRequestHandler<UpdateHostProfileCommand, Response<int>>
{
    private readonly IHostProfileAsyncRepository _hostProfileRepository;
    private readonly IMapper _mapper;
    private readonly IDateTimeService _dateTimeService;

    public UpdateHostProfileCommandHandler(
        IHostProfileAsyncRepository hostProfileRepository, 
        IMapper mapper, 
        IDateTimeService dateTimeService)
    {
        _hostProfileRepository = hostProfileRepository;
        _mapper = mapper;
        _dateTimeService = dateTimeService;
    }

    public async Task<Response<int>> Handle(UpdateHostProfileCommand request, CancellationToken cancellationToken)
    {
        var hostProfile = await _hostProfileRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (hostProfile == null)
        {
            return new Response<int>($"Host profile with ID {request.Id} not found.");
        }

        // Update only provided fields
        if (!string.IsNullOrEmpty(request.CompanyName))
            hostProfile.CompanyName = request.CompanyName;
            
        if (!string.IsNullOrEmpty(request.Description))
            hostProfile.Description = request.Description;
            
        if (!string.IsNullOrEmpty(request.ContactPhone))
            hostProfile.ContactPhone = request.ContactPhone;
            
        if (!string.IsNullOrEmpty(request.LogoUrl))
            hostProfile.LogoUrl = request.LogoUrl;
            
        if (!string.IsNullOrEmpty(request.WebsiteUrl))
            hostProfile.WebsiteUrl = request.WebsiteUrl;
            
        if (request.IsVerified.HasValue)
            hostProfile.IsVerified = request.IsVerified.Value;

        hostProfile.LastModifiedUtc = _dateTimeService.NowUtc;

        try
        {
            await _hostProfileRepository.UpdateAsync(hostProfile, cancellationToken);
            return new Response<int>(hostProfile.Id, "Host profile updated successfully.");
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return new Response<int>($"Update Host profile failed. {msg}");
        }
    }
}

public class UpdateHostProfileCommandValidator : AbstractValidator<UpdateHostProfileCommand>
{
    public UpdateHostProfileCommandValidator()
    {
        RuleFor(p => p.Id)
            .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.");

        RuleFor(p => p.CompanyName)
            .MaximumLength(100).WithMessage("{PropertyName} must not exceed 100 characters.")
            .When(p => !string.IsNullOrEmpty(p.CompanyName));

        RuleFor(p => p.Description)
            .MaximumLength(500).WithMessage("{PropertyName} must not exceed 500 characters.")
            .When(p => !string.IsNullOrEmpty(p.Description));

        RuleFor(p => p.ContactPhone)
            .MaximumLength(15).WithMessage("{PropertyName} must not exceed 15 characters.")
            .When(p => !string.IsNullOrEmpty(p.ContactPhone));

        RuleFor(p => p.LogoUrl)
            .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.")
            .When(p => !string.IsNullOrEmpty(p.LogoUrl));

        RuleFor(p => p.WebsiteUrl)
            .MaximumLength(200).WithMessage("{PropertyName} must not exceed 200 characters.")
            .When(p => !string.IsNullOrEmpty(p.WebsiteUrl));
    }
}
