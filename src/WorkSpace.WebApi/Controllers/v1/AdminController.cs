using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Account;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/admin")]
[Authorize(Roles = nameof(Roles.Admin))]
[ApiController]
public class AdminController : BaseApiController
{
    private readonly IAccountService _accountService;

    public AdminController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // User Management Endpoints
    
    /// <summary>
    /// Lấy danh sách tất cả người dùng với filter và phân trang
    /// </summary>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUsersRequestDto request)
    {
        var result = await _accountService.GetAllUsersAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một người dùng
    /// </summary>
    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserById([FromRoute] int userId)
    {
        var result = await _accountService.GetUserByIdAsync(userId);
        return Ok(result);
    }

    /// <summary>
    /// Tạo tài khoản người dùng mới (chỉ Admin)
    /// </summary>
    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserByAdminDto request)
    {
        var result = await _accountService.CreateUserByAdminAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Ẩn/hiện tài khoản người dùng (cập nhật trạng thái IsActive)
    /// </summary>
    [HttpPut("users/{userId}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        [FromRoute] int userId,
        [FromBody] UpdateUserStatusDto request)
    {
        var result = await _accountService.UpdateUserStatusAsync(userId, request);
        return Ok(result);
    }
}

