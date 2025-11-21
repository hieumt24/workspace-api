using MediatR;
using WorkSpace.Application.DTOs.Owner;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces;
using System.Linq;

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
        private readonly IApplicationDbContext _context;

        public CreateWorkSpaceCommandHandler(
            IWorkSpaceRepository workSpaceRepo,
            IHostProfileAsyncRepository hostRepo,
            IApplicationDbContext context)
        {
            _workSpaceRepo = workSpaceRepo;
            _hostRepo = hostRepo;
            _context = context;
        }

        public async Task<Response<int>> Handle(CreateWorkSpaceCommand request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null)
            {
                throw new ApiException("Owner profile not found.");
            }


            var newAddress = new Address
            {
        
                Street = request.Dto.Street,
                Ward = request.Dto.Ward,
                State = request.Dto.State,
                PostalCode = request.Dto.PostalCode,
                Latitude = request.Dto.Latitude,
                Longitude = request.Dto.Longitude,

         
                Country = "Việt Nam",


                CreateUtc = DateTime.UtcNow,
                CreatedById = request.OwnerUserId
            };

            await _context.Addresses.AddAsync(newAddress, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);


   
            var newWorkSpace = new Domain.Entities.WorkSpace
            {
                Title = request.Dto.Title,
                Description = request.Dto.Description,
                AddressId = newAddress.Id,
                WorkSpaceTypeId = request.Dto.WorkSpaceTypeId,
                HostId = hostProfile.Id,
                IsActive = false,
                IsVerified = false,
                CreateUtc = DateTime.UtcNow,
                CreatedById = request.OwnerUserId
            };

            await _workSpaceRepo.AddAsync(newWorkSpace, cancellationToken);

            if (request.Dto.ImageUrls != null && request.Dto.ImageUrls.Any())
            {
                var images = request.Dto.ImageUrls.Select(url => new WorkSpaceImage
                {
                    WorkSpaceId = newWorkSpace.Id,
                    ImageUrl = url,
                    Caption = newWorkSpace.Title,
                    CreateUtc = DateTime.UtcNow,
                    CreatedById = request.OwnerUserId
                }).ToList();

                await _context.Set<WorkSpaceImage>().AddRangeAsync(images, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return new Response<int>(newWorkSpace.Id, "Workspace created successfully. Waiting for approval.");
        }
    }
}