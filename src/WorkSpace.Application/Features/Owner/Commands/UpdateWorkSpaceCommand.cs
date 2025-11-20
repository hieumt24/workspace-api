using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.Owner;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities; 

namespace WorkSpace.Application.Features.Owner.Commands
{
    public class UpdateWorkSpaceCommand : IRequest<Response<int>>
    {
        public int WorkSpaceId { get; set; }
        public int OwnerUserId { get; set; }
        public UpdateWorkSpaceDto Dto { get; set; }
    }

    public class UpdateWorkSpaceCommandHandler : IRequestHandler<UpdateWorkSpaceCommand, Response<int>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public UpdateWorkSpaceCommandHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
        }

        public async Task<Response<int>> Handle(UpdateWorkSpaceCommand request, CancellationToken cancellationToken)
        {

            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
                throw new ApiException("Không tìm thấy hồ sơ Host.");
            }

            var workspace = await _context.Workspaces
                .Include(w => w.Address)
                .Include(w => w.WorkSpaceImages) 
                .FirstOrDefaultAsync(w => w.Id == request.WorkSpaceId, cancellationToken);

            if (workspace == null)
            {
                throw new ApiException($"Không tìm thấy Workspace ID {request.WorkSpaceId}");
            }

            if (workspace.HostId != hostProfile.Id)
            {
                throw new ApiException("Bạn không có quyền chỉnh sửa Workspace này.");
            }

            var hasAddressChanged = request.Dto.Street != null ||
                                    request.Dto.Ward != null ||
                                    request.Dto.State != null ||
                                    request.Dto.PostalCode != null ||
                                    request.Dto.Latitude.HasValue ||
                                    request.Dto.Longitude.HasValue;

            if (hasAddressChanged)
            {
    
                var currentAddress = workspace.Address;

                var newAddress = new Address
                {
                    Street = request.Dto.Street ?? currentAddress?.Street ?? throw new ApiException("Street là trường bắt buộc."),
                    Ward = request.Dto.Ward ?? currentAddress?.Ward ?? throw new ApiException("Ward là trường bắt buộc."),
                    State = request.Dto.State ?? currentAddress?.State,
                    PostalCode = request.Dto.PostalCode ?? currentAddress?.PostalCode,
                    Latitude = request.Dto.Latitude ?? currentAddress?.Latitude ?? 0,
                    Longitude = request.Dto.Longitude ?? currentAddress?.Longitude ?? 0,
                    Country = currentAddress?.Country ?? "Việt Nam", 

                    CreateUtc = DateTime.UtcNow,
                    CreatedById = request.OwnerUserId 
                };

                await _context.Addresses.AddAsync(newAddress, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                workspace.AddressId = newAddress.Id;
            }



            if (!string.IsNullOrEmpty(request.Dto.Title))
                workspace.Title = request.Dto.Title;

            if (!string.IsNullOrEmpty(request.Dto.Description))
                workspace.Description = request.Dto.Description;

            if (request.Dto.WorkSpaceTypeId.HasValue)
                workspace.WorkSpaceTypeId = request.Dto.WorkSpaceTypeId.Value;

            if (request.Dto.IsActive.HasValue)
                workspace.IsActive = request.Dto.IsActive.Value;



            if (request.Dto.ImageUrls != null)
            {
           
                _context.Set<WorkSpaceImage>().RemoveRange(workspace.WorkSpaceImages);

        
                var newImages = request.Dto.ImageUrls
                    .Where(url => !string.IsNullOrWhiteSpace(url)) 
                    .Select(url => new WorkSpaceImage
                    {
                        WorkSpaceId = workspace.Id,
                        ImageUrl = url,
                        Caption = workspace.Title,
                        CreateUtc = DateTime.UtcNow,
                        CreatedById = request.OwnerUserId
                    }).ToList();

                if (newImages.Any())
                {
                    await _context.Set<WorkSpaceImage>().AddRangeAsync(newImages, cancellationToken);
                }
            }


            workspace.LastModifiedUtc = DateTime.UtcNow;
            workspace.LastModifiedById = request.OwnerUserId;

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<int>(workspace.Id, "Cập nhật Workspace thành công.");
        }
    }
}