namespace BugTrackerBC.Client
{
    // Add properties to this class and update the server and client AuthenticationStateProviders
    // to expose more information about the authenticated user to the client.
    public class UserInfo
    {
        public required string UserId { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string ProfilePictureUrl { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public required string[] Roles { get; set; }
        public required int CompanyId { get; set; }

    }
}

