using BugTrackerBC.Client.Models;

namespace BugTrackerBC.Client.Services.Interfaces
{
    public interface IProjectDTOService
    {
        Task<IEnumerable<ProjectDTO>> GetAllProjectsAsync(int companyId);
        Task<IEnumerable<ProjectDTO>> GetArchivedProjects(int companyId);
        Task<ProjectDTO?> GetProjectByIdAsync(int projectId, int companyId);
        Task<ProjectDTO> AddProjectAsync(ProjectDTO projectDTO, int companyId);
        Task UpdateProjectAsync(ProjectDTO projectDTO, int companyId);
        Task ArchiveProjectAsync(int projectId, int companyId);
        Task RestoreProjectAsync(int projectId, int companyId);
        Task<IEnumerable<UserDTO>> GetProjectMembersAsync(int projectId, int companyId);
        Task<UserDTO?> GetProjectManagerAsync(int projectId, int companyId);
        Task AddMemberToProjectAsync(int projectId, string memberId, string managerId);
        Task RemoveMemberFromProjectAsync(int projectId, string memberId, string managerId);
        Task AssignProjectManagerAsync(int projectId, string memberId, string adminId);
        Task RemoveProjectManagerAsync(int projectId, string adminId);
    }
}
