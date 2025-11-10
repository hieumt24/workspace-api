using WorkSpace.Application.DTOs.Account;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Interfaces.Services;

public interface  IAccountService
{
    Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request, string ipAddress);
    Task<Response<AuthenticationResponse>> RegisterAsync(RegisterRequest request, string origin, string ipAddress);
    Task<Response<string>> ConfirmEmailAsync(string userId, string code);
    Task ForgotPassword(ForgotPasswordRequest model, string origin);
    Task<Response<string>> ResetPassword(ResetPasswordRequest model);
    Task<Response<AuthenticationResponse>> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
    Task<Response<string>> RevokeTokenAsync(string token, string ipAddress);
    
    // Admin functions
    Task<PagedResponse<List<UserDto>>> GetAllUsersAsync(GetAllUsersRequestDto request);
    Task<Response<UserDto>> GetUserByIdAsync(int userId);
    Task<Response<UserDto>> CreateUserByAdminAsync(CreateUserByAdminDto request);
    Task<Response<UserDto>> UpdateUserStatusAsync(int userId, UpdateUserStatusDto request);
}