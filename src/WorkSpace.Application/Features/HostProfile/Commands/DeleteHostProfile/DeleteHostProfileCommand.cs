using MediatR;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using FluentValidation;

namespace WorkSpace.Application.Features.HostProfile.Commands.DeleteHostProfile;

public class DeleteHostProfileCommand : IRequest<Response<int>>
{
    public int Id { get; set; }
    
    public DeleteHostProfileCommand(int id)
    {
        Id = id;
    }
}

public class DeleteHostProfileCommandHandler : IRequestHandler<DeleteHostProfileCommand, Response<int>>
{
    private readonly IHostProfileAsyncRepository _hostProfileRepository;

    public DeleteHostProfileCommandHandler(IHostProfileAsyncRepository hostProfileRepository)
    {
        _hostProfileRepository = hostProfileRepository;
    }

    public async Task<Response<int>> Handle(DeleteHostProfileCommand request, CancellationToken cancellationToken)
    {
        var hostProfile = await _hostProfileRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (hostProfile == null)
        {
            return new Response<int>($"Host profile with ID {request.Id} not found.");
        }

        // Check if host has active workspaces
        var hasActiveWorkspaces = await _hostProfileRepository.HasActiveWorkspacesAsync(request.Id, cancellationToken);
        if (hasActiveWorkspaces)
        {
            return new Response<int>($"Cannot delete host profile with ID {request.Id}. Host has active workspaces.");
        }

        try
        {
            await _hostProfileRepository.DeleteAsync(hostProfile, cancellationToken);
            return new Response<int>(request.Id, "Host profile deleted successfully.");
        }
        catch (DbUpdateException ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            return new Response<int>($"Delete Host profile failed. {msg}");
        }
    }
}

public class DeleteHostProfileCommandValidator : AbstractValidator<DeleteHostProfileCommand>
{
    public DeleteHostProfileCommandValidator()
    {
        RuleFor(p => p.Id)
            .GreaterThan(0).WithMessage("{PropertyName} must be greater than 0.");
    }
}
