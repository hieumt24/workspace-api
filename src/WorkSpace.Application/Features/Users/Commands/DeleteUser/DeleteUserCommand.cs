
using MediatR;
using Microsoft.AspNetCore.Identity;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Exceptions;

namespace WorkSpace.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
    }

    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Response<int>>
    {
        private readonly UserManager<AppUser> _userManager;

        public DeleteUserCommandHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Response<int>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                throw new ApiException($"User with id {request.Id} not found.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new ApiException($"Failed to delete user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return new Response<int>(request.Id, "User deleted successfully.");
        }
    }
}