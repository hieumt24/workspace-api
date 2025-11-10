
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Identity;
using WorkSpace.Application.DTOs.Users;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.Exceptions;

namespace WorkSpace.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<Response<string>>
    {
        public CreateUserRequest CreateUserRequest { get; set; }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Response<string>>
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public CreateUserCommandHandler(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<Response<string>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.CreateUserRequest.UserName);
            if (userWithSameUserName != null)
            {
                throw new ApiException($"Username '{request.CreateUserRequest.UserName}' is already taken.");
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(request.CreateUserRequest.Email);
            if (userWithSameEmail != null)
            {
                throw new ApiException($"Email {request.CreateUserRequest.Email} is already registered.");
            }

            var user = new AppUser
            {
                FirstName = request.CreateUserRequest.FirstName,
                LastName = request.CreateUserRequest.LastName,
                Email = request.CreateUserRequest.Email,
                UserName = request.CreateUserRequest.UserName,
                IsActive = true,
                DateCreated = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.CreateUserRequest.Password);

            if (!result.Succeeded)
            {
                throw new ApiException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            if (request.CreateUserRequest.Roles != null && request.CreateUserRequest.Roles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(user, request.CreateUserRequest.Roles);
                if (!roleResult.Succeeded)
                {
                    throw new ApiException($"Failed to add roles to user: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Basic");
            }


            return new Response<string>(user.Id.ToString(), message: "User created successfully.");
        }
    }
}