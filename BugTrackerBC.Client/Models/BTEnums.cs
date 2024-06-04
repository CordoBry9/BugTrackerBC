// CLIENT PROJECT

using System.ComponentModel.DataAnnotations;
using System.Reflection;

public enum NotificationType
{
    Company,
    Project,
    Ticket,
}

public enum ProjectPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum Roles
{
    Admin,
    [Display(Name = "Project Manager")] ProjectManager,
    Developer,
    Submitter,
    [Display(Name = "Demo User")] DemoUser
}

public enum TicketPriority
{
    Low,
    Medium,
    High,
    Urgent
}

public enum TicketStatus
{
    New,
    [Display(Name = "In Development")] InDevelopment,
    Testing,
    Resolved
}

public enum TicketType
{
    [Display(Name = "New Development")] NewDevelopment,
    [Display(Name = "Work Task")] WorkTask,
    Defect,
    [Display(Name = "Change Request")] ChangeRequest,        
    Enhancement,
    [Display(Name = "General Task")] GeneralTask
}

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        string? displayName = enumValue.GetType()
            .GetMember(enumValue.ToString())
            .FirstOrDefault()?
            .GetCustomAttribute<DisplayAttribute>()?
            .GetName();

        if (string.IsNullOrEmpty(displayName))
        {
            displayName = enumValue.ToString();
        }

        return displayName;
    }
}