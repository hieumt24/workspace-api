using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WorkSpace.Application.DTOs.Account;
using WorkSpace.Application.DTOs.Email;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.ConfigOptions;
using WorkSpace.Domain.Entities;
using WorkSpace.Infrastructure.Helpers;

namespace WorkSpace.Infrastructure.Services;

public class AccountService : IAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly JWTSettings _jwtSettings;
        private readonly GoogleSettings _googleSettings;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<AccountService> _logger;
        
        public AccountService(UserManager<AppUser> userManager, 
            RoleManager<AppRole> roleManager, 
            IOptions<JWTSettings> jwtSettings, 
            IOptions<GoogleSettings> googleSettings,
            IDateTimeService dateTimeService, 
            SignInManager<AppUser> signInManager,
            IEmailService emailService,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtSettings.Value;
            _googleSettings = googleSettings.Value;
            _dateTimeService = dateTimeService;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<Response<AuthenticationResponse>> AuthenticateAsync(AuthenticationRequest request, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Authentication attempt for email: {Email} from IP: {IpAddress}", request.Email, ipAddress);
                
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Authentication failed: No account found for email {Email} from IP {IpAddress}", request.Email, ipAddress);
                    throw new ApiException($"No Accounts Registered with {request.Email}.");
                }
                
                var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Authentication failed: Invalid credentials for email {Email} from IP {IpAddress}", request.Email, ipAddress);
                    throw new ApiException($"Invalid Credentials for '{request.Email}'.");
                }
                
                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("Authentication failed: Email not confirmed for {Email} from IP {IpAddress}", request.Email, ipAddress);
                    throw new ApiException($"Account Not Confirmed for '{request.Email}'.");
                }
                
                JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
                AuthenticationResponse response = new AuthenticationResponse();
                response.Id = user.Id.ToString();
                response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                response.Email = user.Email;
                response.UserName = user.UserName;
                var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
                response.Roles = rolesList.ToList();
                response.IsVerified = user.EmailConfirmed;
                var refreshToken = GenerateRefreshToken(ipAddress);
                response.RefreshToken = refreshToken.Token;
                
                // Save refresh token to database
                user.RefreshToken = refreshToken.Token;
                user.RefreshTokenExpiryTime = refreshToken.Expires;
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                
                _logger.LogInformation("Authentication successful for user {UserId} ({Email}) from IP {IpAddress}", user.Id, user.Email, ipAddress);
                
                return new Response<AuthenticationResponse>(response, $"Authenticated {user.UserName}");
            }
            catch (ApiException)
            {
                throw; // Re-throw ApiException as-is
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during authentication for email {Email} from IP {IpAddress}", request.Email, ipAddress);
                throw new ApiException("An error occurred during authentication. Please try again.");
            }
        }

        public async Task<Response<AuthenticationResponse>> RegisterAsync(RegisterRequest request, string origin, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Registration attempt for email: {Email}, username: {UserName}", request.Email, request.UserName);
                
                // Check if Customer role exists
                var customerRole = Roles.Customer.ToString();
                if (!await _roleManager.RoleExistsAsync(customerRole))
                {
                    _logger.LogError("Customer role does not exist in database. Database may not be seeded properly.");
                    throw new ApiException($"Role '{customerRole}' does not exist. Please contact administrator.");
                }
                
                // Check username
                var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
                if (userWithSameUserName != null)
                {
                    throw new ApiException($"Username '{request.UserName}' is already taken.");
                }
                
                // Check email
                var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
                if (userWithSameEmail != null)
                {
                    throw new ApiException($"Email '{request.Email}' is already registered.");
                }
                
                var user = new AppUser()
                {
                    Email = request.Email,
                    UserName = request.UserName,
                    DateCreated = DateTime.UtcNow,
                    IsActive = true,
                };
                
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("User creation failed for email {Email}: {Errors}", request.Email, errors);
                    throw new ApiException($"Registration failed: {errors}");
                }
                
                // Add to Customer role
                var roleResult = await _userManager.AddToRoleAsync(user, customerRole);
                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to add role '{Role}' to user {UserId}: {Errors}", customerRole, user.Id, errors);
                    
                    // Rollback: delete the user if role assignment fails
                    await _userManager.DeleteAsync(user);
                    throw new ApiException($"Failed to assign customer role: {errors}");
                }
                
                // Auto confirm email for now (skip email verification)
                var emailConfirmToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmResult = await _userManager.ConfirmEmailAsync(user, emailConfirmToken);
                if (!confirmResult.Succeeded)
                {
                    _logger.LogWarning("Email confirmation failed for user {UserId}, but continuing registration", user.Id);
                }
                
                _logger.LogInformation("User registered successfully: {UserId} ({Email})", user.Id, user.Email);
                
                // Auto login after registration - Generate JWT token
                JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
                AuthenticationResponse response = new AuthenticationResponse();
                response.Id = user.Id.ToString();
                response.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                response.Email = user.Email;
                response.UserName = user.UserName;
                var rolesList = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
                response.Roles = rolesList.ToList();
                response.IsVerified = user.EmailConfirmed;
                var refreshToken = GenerateRefreshToken(ipAddress);
                response.RefreshToken = refreshToken.Token;
                
                // Save refresh token to database
                user.RefreshToken = refreshToken.Token;
                user.RefreshTokenExpiryTime = refreshToken.Expires;
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                
                return new Response<AuthenticationResponse>(response, $"User registered and authenticated successfully");
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for email {Email}", request.Email);
                throw new ApiException("An error occurred during registration. Please try again.");
            }
        }

        private async Task<JwtSecurityToken> GenerateJWToken(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
            roleClaims.Add(new Claim("role", roles[i]));
        }

            string ipAddress = IpHelper.GetIpAddress();

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id.ToString()),
                new Claim("ip", ipAddress)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }

        private string RandomTokenString()
        {
            var randomBytes = new byte[40];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }
        
        private async Task<string> SendVerificationEmail(AppUser user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "api/account/confirm-email/";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(_enpointUri.ToString(), "userId", user.Id.ToString());
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            //Email Service Call Here
            return verificationUri;
        }

        public async Task<Response<string>> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if(result.Succeeded)
            {
                return new Response<string>(user.Id.ToString(), message: $"Account Confirmed for {user.Email}. You can now use the /api/Account/authenticate endpoint.");
            }
            else
            {
                throw new ApiException($"An error occured while confirming {user.Email}.");
            }
        }

        private RefreshToken GenerateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = RandomTokenString(),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        public async Task ForgotPassword(ForgotPasswordRequest model, string origin)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);

            if (account == null) throw new ApiException($"No Accounts Registered with {model.Email}.");

            var code = await _userManager.GeneratePasswordResetTokenAsync(account);
            var route = "api/account/reset-password/";
            var _enpointUri = new Uri(string.Concat($"{origin}/", route));
            var emailRequest = new EmailRequest()
            {
                Body = $"You reset token is - {code}",
                To = model.Email,
                Subject = "Reset Password",
            };
            await _emailService.SendAsync(emailRequest);
        }

        public async Task<Response<string>> ResetPassword(ResetPasswordRequest model)
        {
            var account = await _userManager.FindByEmailAsync(model.Email);
            if (account == null) throw new ApiException($"No Accounts Registered with {model.Email}.");
            
            var result = await _userManager.ResetPasswordAsync(account, model.Token, model.Password);
            if(result.Succeeded)
            {
                return new Response<string>(model.Email, message: $"Password Resetted.");
            }
            else
            {
                // Parse specific errors
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                
                // Check for token errors
                if (result.Errors.Any(e => e.Code.Contains("InvalidToken") || e.Description.Contains("Invalid token")))
                {
                    throw new ApiException($"Invalid or expired reset token.");
                }
                
                // Check for password errors
                if (result.Errors.Any(e => e.Code.Contains("Password")))
                {
                    throw new ApiException($"Password validation failed: {errors}");
                }
                
                // Generic error with details
                throw new ApiException($"Error occurred while resetting the password: {errors}");
            }
        }

        public async Task<Response<AuthenticationResponse>> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Refresh token attempt from IP: {IpAddress}", ipAddress);
                
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
                
                if (user == null)
                {
                    _logger.LogWarning("Refresh token failed: Invalid token from IP {IpAddress}", ipAddress);
                    throw new ApiException("Invalid refresh token");
                }

                var refreshToken = new RefreshToken
                {
                    Token = request.RefreshToken,
                    Expires = user.RefreshTokenExpiryTime ?? DateTime.UtcNow.AddDays(-1),
                    Created = DateTime.UtcNow.AddDays(-7), // Assume created 7 days ago
                    CreatedByIp = ipAddress
                };

                if (!refreshToken.IsActive)
                {
                    _logger.LogWarning("Refresh token failed: Token expired for user {UserId} from IP {IpAddress}", user.Id, ipAddress);
                    throw new ApiException("Refresh token has expired");
                }

                // Generate new JWT token
                var jwtToken = await GenerateJWToken(user);
                var newRefreshToken = GenerateRefreshToken(ipAddress);

                // Revoke old refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);

                // Set new refresh token
                user.RefreshToken = newRefreshToken.Token;
                user.RefreshTokenExpiryTime = newRefreshToken.Expires;
                await _userManager.UpdateAsync(user);

                var response = new AuthenticationResponse
                {
                    Id = user.Id.ToString(),
                    JWToken = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                    IsVerified = user.EmailConfirmed,
                    RefreshToken = newRefreshToken.Token
                };

                _logger.LogInformation("Refresh token successful for user {UserId} ({Email}) from IP {IpAddress}", user.Id, user.Email, ipAddress);

                return new Response<AuthenticationResponse>(response, "Token refreshed successfully");
            }
            catch (ApiException)
            {
                throw; // Re-throw ApiException as-is
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh from IP {IpAddress}", ipAddress);
                throw new ApiException("An error occurred during token refresh. Please try again.");
            }
        }

        public async Task<Response<string>> RevokeTokenAsync(string token, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Token revocation attempt from IP: {IpAddress}", ipAddress);
                
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == token);
                
                if (user == null)
                {
                    _logger.LogWarning("Token revocation failed: Invalid token from IP {IpAddress}", ipAddress);
                    throw new ApiException("Invalid refresh token");
                }

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Token revoked successfully for user {UserId} ({Email}) from IP {IpAddress}", user.Id, user.Email, ipAddress);

                return new Response<string>("Token revoked successfully");
            }
            catch (ApiException)
            {
                throw; // Re-throw ApiException as-is
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token revocation from IP {IpAddress}", ipAddress);
                throw new ApiException("An error occurred during token revocation. Please try again.");
            }
        }

        // Admin functions
        public async Task<PagedResponse<List<UserDto>>> GetAllUsersAsync(GetAllUsersRequestDto request)
        {
            try
            {
                _logger.LogInformation("Getting all users - Page: {PageNumber}, PageSize: {PageSize}", request.PageNumber, request.PageSize);
                
                var query = _userManager.Users.AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLower();
                    query = query.Where(u => 
                        u.UserName.ToLower().Contains(searchTerm) || 
                        u.Email.ToLower().Contains(searchTerm) ||
                        (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                        (u.LastName != null && u.LastName.ToLower().Contains(searchTerm))
                    );
                }

                if (request.IsActive.HasValue)
                {
                    query = query.Where(u => u.IsActive == request.IsActive.Value);
                }

                // Get total count before pagination
                var totalRecords = await query.CountAsync();

                // Apply pagination
                var users = await query
                    .OrderByDescending(u => u.DateCreated)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var userDtos = new List<UserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    
                    // Filter by role if specified
                    if (!string.IsNullOrWhiteSpace(request.Role) && !roles.Contains(request.Role))
                    {
                        continue;
                    }

                    userDtos.Add(new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Avatar = user.Avatar,
                        IsActive = user.IsActive,
                        EmailConfirmed = user.EmailConfirmed,
                        DateCreated = user.DateCreated,
                        Dob = user.Dob,
                        LastLoginDate = user.LastLoginDate,
                        Roles = roles.ToList(),
                        FullName = user.GetFullName()
                    });
                }

                _logger.LogInformation("Retrieved {Count} users out of {Total} total", userDtos.Count, totalRecords);

                return new PagedResponse<List<UserDto>>(userDtos, request.PageNumber, request.PageSize, totalRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                throw new ApiException("An error occurred while retrieving users");
            }
        }

        public async Task<Response<UserDto>> GetUserByIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation("Getting user by ID: {UserId}", userId);
                
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    throw new ApiException($"User with ID {userId} not found");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Avatar = user.Avatar,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    DateCreated = user.DateCreated,
                    Dob = user.Dob,
                    LastLoginDate = user.LastLoginDate,
                    Roles = roles.ToList(),
                    FullName = user.GetFullName()
                };

                return new Response<UserDto>(userDto, "User retrieved successfully");
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
                throw new ApiException("An error occurred while retrieving user");
            }
        }

        public async Task<Response<UserDto>> CreateUserByAdminAsync(CreateUserByAdminDto request)
        {
            try
            {
                _logger.LogInformation("Admin creating user with email: {Email}", request.Email);
                
                // Validate role exists
                var roleExists = await _roleManager.RoleExistsAsync(request.Role);
                if (!roleExists)
                {
                    throw new ApiException($"Role '{request.Role}' does not exist");
                }

                // Check if username already exists
                var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
                if (userWithSameUserName != null)
                {
                    throw new ApiException($"Username '{request.UserName}' is already taken");
                }

                // Check if email already exists
                var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
                if (userWithSameEmail != null)
                {
                    throw new ApiException($"Email '{request.Email}' is already registered");
                }

                var user = new AppUser
                {
                    Email = request.Email,
                    UserName = request.UserName,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Dob = request.Dob,
                    DateCreated = DateTime.UtcNow,
                    IsActive = request.IsActive,
                    EmailConfirmed = request.EmailConfirmed
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("User creation failed for email {Email}: {Errors}", request.Email, errors);
                    throw new ApiException($"User creation failed: {errors}");
                }

                // Add role
                await _userManager.AddToRoleAsync(user, request.Role);

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Avatar = user.Avatar,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    DateCreated = user.DateCreated,
                    Dob = user.Dob,
                    LastLoginDate = user.LastLoginDate,
                    Roles = roles.ToList(),
                    FullName = user.GetFullName()
                };

                _logger.LogInformation("User created successfully by admin: {UserId} ({Email})", user.Id, user.Email);

                return new Response<UserDto>(userDto, "User created successfully");
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user by admin");
                throw new ApiException("An error occurred while creating user");
            }
        }

        public async Task<Response<UserDto>> UpdateUserStatusAsync(int userId, UpdateUserStatusDto request)
        {
            try
            {
                _logger.LogInformation("Updating user status for user ID: {UserId} to {IsActive}", userId, request.IsActive);
                
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    throw new ApiException($"User with ID {userId} not found");
                }

                user.IsActive = request.IsActive;
                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("User status update failed for user {UserId}: {Errors}", userId, errors);
                    throw new ApiException($"User status update failed: {errors}");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Avatar = user.Avatar,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    DateCreated = user.DateCreated,
                    Dob = user.Dob,
                    LastLoginDate = user.LastLoginDate,
                    Roles = roles.ToList(),
                    FullName = user.GetFullName()
                };

                _logger.LogInformation("User status updated successfully for user {UserId}", userId);

                return new Response<UserDto>(userDto, $"User status updated to {(request.IsActive ? "Active" : "Inactive")}");
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user status for user ID: {UserId}", userId);
                throw new ApiException("An error occurred while updating user status");
            }
        }

        public async Task<Response<AuthenticationResponse>> GoogleLoginAsync(GoogleLoginRequest request, string ipAddress)
        {
            try
            {
                _logger.LogInformation("Google login attempt from IP: {IpAddress}", ipAddress);
                
                // Verify Google ID Token
                GoogleJsonWebSignature.Payload payload;
                try
                {
                    var settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { _googleSettings.ClientId }
                    };
                    
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Invalid Google ID token");
                    throw new ApiException("Invalid Google token");
                }
                
                if (string.IsNullOrEmpty(payload.Email))
                {
                    throw new ApiException("Email not provided by Google");
                }
                
                // Check if user exists
                var user = await _userManager.FindByEmailAsync(payload.Email);
                
                if (user == null)
                {
                    // Create new user
                    _logger.LogInformation("Creating new user from Google login: {Email}", payload.Email);
                    
                    // Check if Customer role exists
                    var customerRole = Roles.Customer.ToString();
                    if (!await _roleManager.RoleExistsAsync(customerRole))
                    {
                        _logger.LogError("Customer role does not exist in database");
                        throw new ApiException($"Role '{customerRole}' does not exist. Please contact administrator.");
                    }
                    
                    user = new AppUser
                    {
                        Email = payload.Email,
                        UserName = payload.Email,
                        EmailConfirmed = true, // Auto-confirm since Google verified it
                        FirstName = payload.GivenName ?? "",
                        LastName = payload.FamilyName ?? "",
                        Avatar = payload.Picture,
                        DateCreated = DateTime.UtcNow,
                        IsActive = true
                    };
                    
                    var result = await _userManager.CreateAsync(user);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to create user from Google login: {Errors}", errors);
                        throw new ApiException($"User creation failed: {errors}");
                    }
                    
                    // Add to Customer role
                    var roleResult = await _userManager.AddToRoleAsync(user, customerRole);
                    if (!roleResult.Succeeded)
                    {
                        var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                        _logger.LogError("Failed to add role to Google user {UserId}: {Errors}", user.Id, errors);
                        
                        // Rollback: delete the user
                        await _userManager.DeleteAsync(user);
                        throw new ApiException($"Failed to assign customer role: {errors}");
                    }
                    
                    _logger.LogInformation("New user created from Google login: {UserId} ({Email})", user.Id, user.Email);
                }
                else
                {
                    // Check if user is active
                    if (!user.IsActive)
                    {
                        _logger.LogWarning("Inactive user attempted Google login: {Email}", user.Email);
                        throw new ApiException("Your account has been deactivated. Please contact support.");
                    }
                    
                    _logger.LogInformation("Existing user logging in via Google: {UserId} ({Email})", user.Id, user.Email);
                }
                
                // Generate JWT token
                JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);
                AuthenticationResponse response = new AuthenticationResponse
                {
                    Id = user.Id.ToString(),
                    JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                    Email = user.Email,
                    UserName = user.UserName,
                    Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                    IsVerified = user.EmailConfirmed
                };
                
                var refreshToken = GenerateRefreshToken(ipAddress);
                response.RefreshToken = refreshToken.Token;
                
                // Save refresh token to database
                user.RefreshToken = refreshToken.Token;
                user.RefreshTokenExpiryTime = refreshToken.Expires;
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                
                _logger.LogInformation("Google login successful for user {UserId} ({Email})", user.Id, user.Email);
                
                return new Response<AuthenticationResponse>(response, "Google authentication successful");
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google login");
                throw new ApiException("An error occurred during Google authentication. Please try again.");
            }
        }
    }