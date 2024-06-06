using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using BugTrackerBC.Models;
using BugTrackerBC.Services.Interfaces;

namespace BugTrackerBC.Services
{
    public class ProjectDTOService : IProjectDTOService
    {
        private readonly IProjectRepository _repository;

        public ProjectDTOService(IProjectRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProjectDTO> AddProjectAsync(ProjectDTO projectDTO, int companyId)
        {
            Project newproject = new Project()
            {
                Name = projectDTO.Name,
                Description = projectDTO.Description,
                Created = DateTime.Now,
                StartDate = projectDTO.StartDate.UtcDateTime,
                EndDate = projectDTO.EndDate.UtcDateTime,
                Priority = projectDTO.Priority,
                Archived = projectDTO.Archived,
                CompanyId = companyId
            };

            if (newproject.CompanyId != companyId)
            {
                Console.WriteLine("User does not have permission to create a project for this company.");
            }
       
                Project createdProject = await _repository.AddProjectAsync(newproject, companyId);
                return createdProject.ToDTO();

        }
        public async Task<IEnumerable<ProjectDTO>> GetAllProjectsAsync(int companyId)
        {
            IEnumerable<Project> projects = await _repository.GetAllProjectsAsync(companyId);
            return projects.Select(p => p.ToDTO()).ToList();
        }

        public async Task ArchiveProjectAsync(int projectId, int companyId)
        {
            await _repository.ArchiveProjectAsync(projectId, companyId);
        }


        public async Task<IEnumerable<ProjectDTO>> GetArchivedProjects(int companyId)
        {
            IEnumerable<Project> archivedProjects = await _repository.GetArchivedProjectsAsync(companyId);
            return archivedProjects.Select(p => p.ToDTO()).ToList();
        }

        public async Task<ProjectDTO?> GetProjectByIdAsync(int projectId, int companyId)
        {
            Project? project = await _repository.GetProjectByIdAsync(projectId, companyId);
            return project?.ToDTO();
        }

        public async Task RestoreProjectAsync(int projectId, int companyId)
        {
            await _repository.RestoreProjectAsync(projectId, companyId);
        }

        public async Task UpdateProjectAsync(ProjectDTO projectDTO, int companyId)
        {
            Project updatedProject = new Project
            {
                Id = projectDTO.Id,
                Name = projectDTO.Name,
                Description = projectDTO.Description,
                Priority = projectDTO.Priority,
                StartDate = projectDTO.StartDate.UtcDateTime,
                EndDate = projectDTO.EndDate.UtcDateTime,
                Archived = projectDTO.Archived,
                CompanyId = companyId
            };

            await _repository.UpdateProjectAsync(updatedProject, companyId);
        }

        public Task<IEnumerable<UserDTO>> GetProjectMembersAsync(int projectId, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO?> GetProjectManagerAsync(int projectId, int companyId)
        {
            throw new NotImplementedException();
        }

        public Task AddMemberToProjectAsync(int projectId, string memberId, string managerId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveMemberFromProjectAsync(int projectId, string memberId, string managerId)
        {
            throw new NotImplementedException();
        }

        public Task AssignProjectManagerAsync(int projectId, string memberId, string adminId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveProjectManagerAsync(int projectId, string adminId)
        {
            throw new NotImplementedException();
        }
    }
}
