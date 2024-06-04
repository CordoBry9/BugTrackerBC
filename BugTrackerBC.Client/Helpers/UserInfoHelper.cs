using BugTrackerBC.Client;
using Microsoft.AspNetCore.Components.Authorization;
using System.ComponentModel.Design;
using System.Security.Claims;

public static class UserInfoHelper
{
    public static UserInfo? GetUserInfo(AuthenticationState authState)
    {
        string? userId = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        string? email = authState.User.FindFirst(ClaimTypes.Email)?.Value;
        string? firstName = authState.User.FindFirst(nameof(UserInfo.FirstName))?.Value;
        string? lastName = authState.User.FindFirst(nameof(UserInfo.LastName))?.Value;
        string? profilePictureUrl = authState.User.FindFirst(nameof(UserInfo.ProfilePictureUrl))?.Value;


        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(firstName)
            || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(profilePictureUrl))
        {
            return null;
        }

        int companyId = int.Parse(authState.User.FindFirst("CompanyId")!.Value);
        return new UserInfo
        {
            UserId = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            ProfilePictureUrl = profilePictureUrl,
            Roles = [.. authState.User.FindAll(ClaimTypes.Role).Select(c => c.Value)],
            CompanyId = companyId

        };
    }

    public static async Task<UserInfo?> GetUserInfoAsync(Task<AuthenticationState>? authStateTask)
    {
        if (authStateTask is null)
        {
            return null;
        }
        else
        {
            AuthenticationState authState = await authStateTask;
            UserInfo? userInfo = GetUserInfo(authState);
            return userInfo;
        }
    }
}