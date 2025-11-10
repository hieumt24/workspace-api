using MediatR;
using WorkSpace.Application.DTOs.Owner;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace WorkSpace.Application.Features.Owner.Commands
{
    public class CreateWorkSpaceCommand : IRequest<Response<int>>
    {
        public int OwnerUserId { get; set; }
        public CreateWorkSpaceDto Dto { get; set; }
    }

    public class CreateWorkSpaceCommandHandler : IRequestHandler<CreateWorkSpaceCommand, Response<int>>
    {
        private readonly IWorkSpaceRepository _workSpaceRepo;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public CreateWorkSpaceCommandHandler(IWorkSpaceRepository workSpaceRepo, IHostProfileAsyncRepository hostRepo)
        {
            _workSpaceRepo = workSpaceRepo;
            _hostRepo = hostRepo;
        }

        public async Task<Response<int>> Handle(CreateWorkSpaceCommand request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
                throw new ApiException("Owner profile not found.");
            }

            var newWorkSpace = new Domain.Entities.WorkSpace
            {
                Title = request.Dto.Title,
                Description = request.Dto.Description,
                AddressId = request.Dto.AddressId,
                WorkSpaceTypeId = request.Dto.WorkSpaceTypeId,
                HostId = hostProfile.Id,
                IsActive = false, 
                IsVerified = false,
                CreateUtc = DateTime.UtcNow
            };

            await _workSpaceRepo.AddAsync(newWorkSpace, cancellationToken);
            return new Response<int>(newWorkSpace.Id, "Workspace created successfully. Waiting for approval.");
        }
    }
}