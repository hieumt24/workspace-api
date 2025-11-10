using System.Security.Claims;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Services;
 
public class AuthenticatedUserService : IAuthenticatedUserService
{
    public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
    {
        UserId = httpContextAccessor.HttpContext?.User?.FindFirstValue("uid");
    }

    public string UserId { get; }
}