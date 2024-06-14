using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using BugTrackerBC.Data;
using BugTrackerBC.Models;
using BugTrackerBC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BugTrackerBC.Services
{
    public class ProjectDTOService : IProjectDTOService
    {
        
        private readonly IProjectRepository _repository;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProjectDTOService(IProjectRepository repository, ICompanyRepository companyRepository, UserManager<ApplicationUser> userManager)
        {
            _repository = repository;
            _companyRepository = companyRepository;
            _userManager = userManager;
        }

        public async Task<ProjectDTO> AddProjectAsync(ProjectDTO projectDTO, int companyId, string userId)
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
                CompanyId = companyId,
                
            };

            if (newproject.CompanyId != companyId)
            {
                Console.WriteLine("User does not have permission to create a project for this company.");
            }

                Project createdProject = await _repository.AddProjectAsync(newproject, companyId, userId);
                return createdProject.ToDTO();

        }
        public async Task<IEnumerable<ProjectDTO>> GetAllProjectsAsync(int companyId)
        {
            IEnumerable<Project> projects = await _repository.GetAllProjectsAsync(companyId);
            return projects.Select(p => p.ToDTO()).ToList();
        }

        public async Task<IEnumerable<ProjectDTO>> GetMemberProjectsAsync(int companyId, string memberId)
        {
            IEnumerable<Project> projects = await _repository.GetMemberProjectsAsync(companyId, memberId);
            return projects.Select(p =>p.ToDTO()).ToList();
        }

        public async Task<IEnumerable<ProjectDTO>> GetMemberArchivedProjectsAsync(int companyId, string memberId)
        {
            IEnumerable<Project> archivedProjects = await _repository.GetMemberArchivedProjectsAsync(companyId, memberId);
            return archivedProjects.Select(p => p.ToDTO()).ToList();
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
        public async Task ArchiveProjectAsync(int projectId, int companyId)
        {
            await _repository.ArchiveProjectAsync(projectId, companyId);
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

        public async Task<IEnumerable<UserDTO>> GetProjectMembersAsync(int projectId, int companyId)
        {
            IEnumerable<ApplicationUser> members = await _repository.GetProjectMembersAsync(projectId, companyId);

            List<UserDTO> result = [];

            foreach (ApplicationUser member in members)
            {
                UserDTO userDTO = member.ToDTO();
                userDTO.Role = await _companyRepository.GetUserRoleAsync(member.Id, companyId);
                result.Add(userDTO);
            }

            return result;
        }

        public async Task<UserDTO?> GetProjectManagerAsync(int projectId, int companyId)
        {
            ApplicationUser? projectManager = await _repository.GetProjectManagerAsync(projectId,companyId);
            if (projectManager == null) return null;

            UserDTO userDTO = projectManager.ToDTO();
            userDTO.Role = nameof(Roles.ProjectManager);

            return userDTO;
        }

        public async Task AddMemberToProjectAsync(int projectId, string memberId, string managerId)
        {
            await _repository.AddMemberToProjectAsync(projectId, memberId, managerId);
        }

        public async Task RemoveMemberFromProjectAsync(int projectId, string memberId, string managerId)
        {
            await _repository.RemoveMemberFromProjectAsync(projectId, memberId, managerId);    
        }

        public async Task AssignProjectManagerAsync(int projectId, string memberId, string adminId)
        {
            await _repository.AssignProjectManagerAsync(projectId, memberId, adminId);
        }

        public async Task RemoveProjectManagerAsync(int projectId, string adminId)
        {
           await _repository.RemoveProjectManagerAsync(projectId, adminId);
        }

    }
}
