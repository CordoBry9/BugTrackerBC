using BugTrackerBC.Models;

namespace BugTrackerBC.Services.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllProjectsAsync(int companyId);
        Task<IEnumerable<Project>> GetArchivedProjectsAsync(int companyId);
        Task<Project?> GetProjectByIdAsync(int projectId, int companyId);
        Task<Project> AddProjectAsync(Project project, int companyId);
        Task UpdateProjectAsync(Project project, int companyId);
        Task ArchiveProjectAsync(int projectId, int companyId);
        Task RestoreProjectAsync(int projectId, int companyId);

    }
}
