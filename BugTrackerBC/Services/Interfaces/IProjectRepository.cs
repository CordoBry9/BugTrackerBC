using BugTrackerBC.Data;
using BugTrackerBC.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.ExpressionTranslators.Internal;

namespace BugTrackerBC.Services.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync(int companyId);
        Task<IEnumerable<Project>> GetMemberProjectsAsync(int companyId, string memberId);
        Task<IEnumerable<Project>> GetMemberArchivedProjectsAsync(int companyId, string memberId);
        Task<IEnumerable<Project>> GetArchivedProjectsAsync(int companyId);
        Task<Project?> GetProjectByIdAsync(int projectId, int companyId);
        Task<Project> AddProjectAsync(Project project, int companyId);
        Task UpdateProjectAsync(Project project, int companyId);
        Task ArchiveProjectAsync(int projectId, int companyId);
        Task RestoreProjectAsync(int projectId, int companyId);
        Task<IEnumerable<ApplicationUser>> GetProjectMembersAsync(int projectId, int companyId);
        Task<ApplicationUser?> GetProjectManagerAsync(int projectId, int companyId);
        Task AddMemberToProjectAsync(int projectId, string userId, string managerId);
        Task RemoveMemberFromProjectAsync(int projectId, string userId, string managerId);
        Task AssignProjectManagerAsync(int projectId, string userId, string adminId);
        Task RemoveProjectManagerAsync(int projectId, string adminId);



    }
}
