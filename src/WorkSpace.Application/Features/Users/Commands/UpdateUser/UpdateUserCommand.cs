

using MediatR;
using Microsoft.AspNetCore.Identity;
using WorkSpace.Application.DTOs.Users;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Exceptions;
using System.Linq;

namespace WorkSpace.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
        public UpdateUserRequest UpdateUserRequest { get; set; }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Response<int>>
    {
        private readonly UserManager<AppUser> _userManager;

        public UpdateUserCommandHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Response<int>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                throw new ApiException($"User with id {request.Id} not found.");
            }

            user.FirstName = request.UpdateUserRequest.FirstName;
            user.LastName = request.UpdateUserRequest.LastName;
            user.PhoneNumber = request.UpdateUserRequest.PhoneNumber;
            user.IsActive = request.UpdateUserRequest.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new ApiException($"Failed to update user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var resultRoles = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!resultRoles.Succeeded)
            {
                throw new ApiException($"Failed to remove user roles: {string.Join(", ", resultRoles.Errors.Select(e => e.Description))}");
            }

            resultRoles = await _userManager.AddToRolesAsync(user, request.UpdateUserRequest.Roles);

            if (!resultRoles.Succeeded)
            {
                throw new ApiException($"Failed to add user roles: {string.Join(", ", resultRoles.Errors.Select(e => e.Description))}");
            }


            return new Response<int>(user.Id, "User updated successfully.");
        }
    }
}