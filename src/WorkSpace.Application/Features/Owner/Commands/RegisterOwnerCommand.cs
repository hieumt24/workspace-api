using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using WorkSpace.Application.DTOs.Owner;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

// Alias để tránh nhầm lẫn
using HostProfileEntity = WorkSpace.Domain.Entities.HostProfile;

namespace WorkSpace.Application.Features.Owner.Commands
{
    public class RegisterOwnerCommand : IRequest<Response<int>>
    {
        [JsonIgnore]
        public int UserId { get; set; }
        public RegisterOwnerDto Dto { get; set; } = null!;
    }

    public class RegisterOwnerCommandHandler : IRequestHandler<RegisterOwnerCommand, Response<int>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHostProfileAsyncRepository _hostRepo;
        private readonly IDateTimeService _dateTimeService;

        public RegisterOwnerCommandHandler(
            UserManager<AppUser> userManager,
            IHostProfileAsyncRepository hostRepo,
            IDateTimeService dateTimeService)
        {
            _userManager = userManager;
            _hostRepo = hostRepo;
            _dateTimeService = dateTimeService;
        }

        public async Task<Response<int>> Handle(RegisterOwnerCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null) throw new ApiException("User not found.");

            var existingProfile = await _hostRepo.GetHostProfileByUserId(request.UserId, cancellationToken);
            if (existingProfile != null)
            {
                return new Response<int>($"User already has a host profile (ID: {existingProfile.Id}).");
            }

       
            var hostProfile = new HostProfileEntity
            {
                UserId = request.UserId,
                CompanyName = request.Dto.CompanyName,
                Description = request.Dto.Description,
                ContactPhone = request.Dto.ContactPhone,
                LogoUrl = request.Dto.LogoUrl,
                WebsiteUrl = request.Dto.WebsiteUrl,
                IsVerified = false,
                CreateUtc = _dateTimeService.NowUtc,
                CreatedById = request.UserId
            };

            
            if (request.Dto.DocumentUrls != null && request.Dto.DocumentUrls.Any())
            {
                foreach (var docUrl in request.Dto.DocumentUrls)
                {
            
                    hostProfile.Documents.Add(new HostProfileDocument
                    {
                        FileUrl = docUrl
                    });
                }
            }


            await _hostRepo.AddAsync(hostProfile, cancellationToken);

            return new Response<int>(hostProfile.Id, "Registered as Owner successfully.");
        }
    }
}