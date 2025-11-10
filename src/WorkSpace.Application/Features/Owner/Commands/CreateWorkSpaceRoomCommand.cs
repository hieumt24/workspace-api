using MediatR;
using WorkSpace.Application.DTOs.Owner;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace WorkSpace.Application.Features.Owner.Commands
{
    public class CreateWorkSpaceRoomCommand : IRequest<Response<int>>
    {
        public int OwnerUserId { get; set; }
        public int WorkspaceId { get; set; }
        public CreateWorkSpaceRoomDto Dto { get; set; }
    }

    public class CreateWorkSpaceRoomCommandHandler : IRequestHandler<CreateWorkSpaceRoomCommand, Response<int>>
    {
        private readonly IGenericRepositoryAsync<WorkSpaceRoom> _roomRepo;
        private readonly IWorkSpaceRepository _workSpaceRepo;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public CreateWorkSpaceRoomCommandHandler(IGenericRepositoryAsync<WorkSpaceRoom> roomRepo, IWorkSpaceRepository workSpaceRepo, IHostProfileAsyncRepository hostRepo)
        {
            _roomRepo = roomRepo;
            _workSpaceRepo = workSpaceRepo;
            _hostRepo = hostRepo;
        }

        public async Task<Response<int>> Handle(CreateWorkSpaceRoomCommand request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            var workspace = await _workSpaceRepo.GetByIdAsync(request.WorkspaceId, cancellationToken);

            if (workspace == null || hostProfile == null || workspace.HostId != hostProfile.Id)
            {
                throw new ApiException("Invalid workspace or permission denied.");
            }

            var newRoom = new WorkSpaceRoom
            {
                WorkSpaceId = request.WorkspaceId,
                Title = request.Dto.Title,
                Description = request.Dto.Description,
                WorkSpaceRoomTypeId = request.Dto.WorkSpaceRoomTypeId,
                PricePerHour = request.Dto.PricePerHour,
                PricePerDay = request.Dto.PricePerDay,
                PricePerMonth = request.Dto.PricePerMonth,
                Capacity = request.Dto.Capacity,
                Area = request.Dto.Area,
                IsActive = true,
                IsVerified = true, 
                CreateUtc = DateTime.UtcNow
            };

            await _roomRepo.AddAsync(newRoom, cancellationToken);
            return new Response<int>(newRoom.Id, "Workspace room created successfully.");
        }
    }
}