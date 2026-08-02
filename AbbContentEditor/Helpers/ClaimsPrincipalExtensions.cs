using System.Security.Claims;

namespace AbbContentEditor
{
    public static class ClaimsPrincipalExtensions
    {
        // Gets the Username from standard identity claims
        public static string? GetUsername(this ClaimsPrincipal user)
        {
            return user.Identity?.Name ?? user.FindFirst(ClaimTypes.Name)?.Value;
        }

        // Gets the User ID directly from claims (saves DB calls if stored in JWT)
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
