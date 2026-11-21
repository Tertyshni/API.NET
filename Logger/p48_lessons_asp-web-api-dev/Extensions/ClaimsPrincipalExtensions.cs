using System;
using System.Security.Claims;
using p48_lessons_asp.Data.Enum;

namespace p48_lessons_asp.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
    public static string? GetUserEmail(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email);
    }

   public static UserRole? GetUserRole(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.Role);
        
        if (Enum.TryParse<UserRole>(value, ignoreCase: true, out var role) && Enum.IsDefined(role))
        {
            return role;
        }

        return null;
    }
}
