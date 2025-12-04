using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Users;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/users")]
    [ApiController]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Staff)}")]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService; 

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetListUsers([FromQuery] string? searchTerm)
        {
           
            var result = await _userService.GetAllUsersAsync(searchTerm);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetail(int id)
        {
        
            var result = await _userService.GetUserByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewUser([FromBody] CreateUserRequest request)
        {
       
            var result = await _userService.CreateUserAsync(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserInfo(int id, [FromBody] UpdateUserRequest request)
        {
     
            var result = await _userService.UpdateUserAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAccount(int id)
        {

            var result = await _userService.DeleteUserAsync(id);
            return Ok(result);
        }
    }
}