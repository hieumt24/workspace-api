using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WorkSpace.Application.Extensions
{
    public static class ClaimExtensions
    {
        // Lấy user id
        public static int GetUserId(this ClaimsPrincipal user)
        {
            if (user == null)
                return 0;

            // Try to find "uid" claim (case-insensitive)
            var idClaim = user.Claims.FirstOrDefault(c => 
                c.Type.Equals("uid", StringComparison.OrdinalIgnoreCase));
            
            if (idClaim != null && int.TryParse(idClaim.Value, out int userId))
                return userId;

            // Fallback: try other common claim types
            idClaim = user.Claims.FirstOrDefault(c =>
                c.Type.Equals("sub", StringComparison.OrdinalIgnoreCase) ||
                c.Type == ClaimTypes.NameIdentifier);

            if (idClaim != null && int.TryParse(idClaim.Value, out userId))
                return userId;

            return 0;
        }




        // Lấy user name 
        public static string GetUserName(this ClaimsPrincipal user)
        {
            if (user == null) return null;

            return user.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.GivenName ||
                c.Type == ClaimTypes.Name ||
                c.Type == JwtRegisteredClaimNames.Name ||
                string.Equals(c.Type, JwtRegisteredClaimNames.Sub, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Type, "sub", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Type, "email", StringComparison.OrdinalIgnoreCase)
            )?.Value;
        }

        // Lấy email
        public static string GetUserEmail(this ClaimsPrincipal user)
        {
            if (user == null) return null;

            return user.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.Email ||
                string.Equals(c.Type, JwtRegisteredClaimNames.Email, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Type, "email", StringComparison.OrdinalIgnoreCase)
            )?.Value;
        }

        // Lấy roles
        public static string[] GetRoles(this ClaimsPrincipal user)
        {
            if (user == null) return Array.Empty<string>();

            var roleClaims = user.Claims.Where(c =>
                c.Type == ClaimTypes.Role ||
                string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Type, "roles", StringComparison.OrdinalIgnoreCase)
            ).Select(c => c.Value).ToList();

            if (roleClaims.Count == 0) return Array.Empty<string>();

            if (roleClaims.Count == 1 && roleClaims[0].Contains(","))
            {
                return roleClaims[0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(r => r.Trim()).ToArray();
            }

            return roleClaims.SelectMany(v => v.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                              .Select(r => r.Trim()))
                             .Where(r => !string.IsNullOrEmpty(r))
                             .ToArray();
        }

        // Lấy expiration (exp) 
        public static DateTimeOffset? GetExpiration(this ClaimsPrincipal user)
        {
            if (user == null) return null;

            var expClaim = user.Claims.FirstOrDefault(c =>
                string.Equals(c.Type, JwtRegisteredClaimNames.Exp, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Type, "exp", StringComparison.OrdinalIgnoreCase)
            )?.Value;

            if (string.IsNullOrEmpty(expClaim)) return null;

            if (long.TryParse(expClaim, out var secondsSinceEpoch))
            {
                return DateTimeOffset.FromUnixTimeSeconds(secondsSinceEpoch);
            }

            if (DateTimeOffset.TryParse(expClaim, out var dt)) return dt;

            return null;
        }
    }
}
