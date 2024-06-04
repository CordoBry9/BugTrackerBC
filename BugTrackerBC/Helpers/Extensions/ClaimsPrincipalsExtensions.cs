using System.Security.Claims;

namespace BugTrackerBC.Helpers.Extensions
{
    public static class ClaimsPrincipalsExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
    