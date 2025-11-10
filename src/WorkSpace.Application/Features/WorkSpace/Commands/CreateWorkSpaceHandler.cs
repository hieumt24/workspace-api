using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.WorkSpace.Commands;

public record CreateWorkSpaceCommand(CreateWorkSpaceRequest Model) : IRequest<Response<int>>;

public class CreateWorkSpaceHandler(IWorkSpaceRepository repository,
    IMapper mapper ) : IRequestHandler<CreateWorkSpaceCommand, Response<int>>
{
    public async Task<Response<int>> Handle(CreateWorkSpaceCommand request, CancellationToken cancellationToken)
    {
        var exists = await repository.ExistsTitleForHostAsync(request.Model.HostId,request.Model.Title, cancellationToken);
        if (exists)
        {
            return new Response<int>($"HostId {request.Model.HostId} đã có workspace với Title {request.Model.Title}");
        }
        var entity = mapper.Map<Domain.Entities.WorkSpace>(request.Model);
        entity.CreateUtc = DateTime.UtcNow;
        try
        {
            await repository.AddAsync(entity, cancellationToken);
            return new Response<int>(entity.Id, "Tạo mới workspace thành công");
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return new Response<int>($"Save workspace failed. {msg}");
        }
    }
}