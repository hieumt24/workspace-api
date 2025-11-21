using MediatR;
using WorkSpace.Application.DTOs.Owner;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces; 

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
        private readonly IApplicationDbContext _context; 

        public CreateWorkSpaceRoomCommandHandler(
            IGenericRepositoryAsync<WorkSpaceRoom> roomRepo,
            IWorkSpaceRepository workSpaceRepo,
            IHostProfileAsyncRepository hostRepo,
            IApplicationDbContext context) 
        {
            _roomRepo = roomRepo;
            _workSpaceRepo = workSpaceRepo;
            _hostRepo = hostRepo;
            _context = context;
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
                CreateUtc = DateTime.UtcNow,
                CreatedById = request.OwnerUserId
            };

            await _roomRepo.AddAsync(newRoom, cancellationToken);

            // 2. Add Images
            if (request.Dto.ImageUrls != null && request.Dto.ImageUrls.Any())
            {
                var images = request.Dto.ImageUrls.Select(url => new WorkSpaceRoomImage
                {
                    WorkSpaceRoomId = newRoom.Id,
                    ImageUrl = url,
                    Caption = newRoom.Title, 
                    CreateUtc = DateTime.UtcNow,
                    CreatedById = request.OwnerUserId
                }).ToList();

                await _context.Set<WorkSpaceRoomImage>().AddRangeAsync(images, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return new Response<int>(newRoom.Id, "Workspace room created successfully.");
        }
    }
}