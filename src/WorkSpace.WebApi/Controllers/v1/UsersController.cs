using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Users;
using WorkSpace.Application.Features.Users.Commands.CreateUser;
using WorkSpace.Application.Features.Users.Commands.DeleteUser;
using WorkSpace.Application.Features.Users.Commands.UpdateUser;
using WorkSpace.Application.Features.Users.Queries.GetAllUsers;
using WorkSpace.Application.Features.Users.Queries.GetUserById;
using WorkSpace.Application.Features.Users.Queries.GetCurrentUser;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/users")]
    [ApiController]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Staff)}")]
    public class UsersController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllUsersQuery query, CancellationToken cancellationToken)
        {
            if (Mediator == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Mediator not initialized.");
            }
            return Ok(await Mediator.Send(query, cancellationToken));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            if (Mediator == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Mediator not initialized.");

            return Ok(await Mediator.Send(new GetUserByIdQuery { Id = id }, cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
        {
            if (Mediator == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Mediator not initialized.");

            return Ok(await Mediator.Send(new CreateUserCommand { CreateUserRequest = request }, cancellationToken));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
        {
            if (Mediator == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Mediator not initialized.");

            return Ok(await Mediator.Send(new UpdateUserCommand { Id = id, UpdateUserRequest = request }, cancellationToken));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            if (Mediator == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Mediator not initialized.");

            return Ok(await Mediator.Send(new DeleteUserCommand { Id = id }, cancellationToken));
        }
    }
}
