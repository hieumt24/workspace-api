using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.HostProfile.Commands.ApproveHostProfile
{
    public class ApproveHostProfileCommand : IRequest<Response<bool>>
    {
        public int HostProfileId { get; set; }
        public bool IsApproved { get; set; }
    }

    public class ApproveHostProfileHandler : IRequestHandler<ApproveHostProfileCommand, Response<bool>>
    {
        private readonly IHostProfileAsyncRepository _hostProfileRepository;
        private readonly UserManager<AppUser> _userManager;

        public ApproveHostProfileHandler(
            IHostProfileAsyncRepository hostProfileRepository,
            UserManager<AppUser> userManager)
        {
            _hostProfileRepository = hostProfileRepository;
            _userManager = userManager;
        }

        public async Task<Response<bool>> Handle(ApproveHostProfileCommand request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostProfileRepository.GetByIdAsync(request.HostProfileId, cancellationToken);
            if (hostProfile == null)
            {
                return new Response<bool>("Không tìm thấy hồ sơ Host.");
            }

            hostProfile.IsVerified = request.IsApproved;
            hostProfile.LastModifiedUtc = DateTime.UtcNow;

            await _hostProfileRepository.UpdateAsync(hostProfile, cancellationToken);

            if (request.IsApproved)
            {
                var user = await _userManager.FindByIdAsync(hostProfile.UserId.ToString());
                if (user != null)
                {
                   
                    var currentRoles = await _userManager.GetRolesAsync(user);

   
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    }

                   
                    await _userManager.AddToRoleAsync(user, nameof(Roles.Owner));
                }
            }

            var message = request.IsApproved
                ? "Đã duyệt hồ sơ. Tài khoản đã chuyển sang quyền Owner."
                : "Đã từ chối hồ sơ Host.";

            return new Response<bool>(true, message);
        }
    }
}