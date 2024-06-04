using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using System.Net.Http.Json;

namespace BugTrackerBC.Client.Services
{
    public class WASMProjectDTOService : IProjectDTOService
    {

        private readonly HttpClient _httpClient;

        public WASMProjectDTOService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProjectDTO> AddProjectAsync(ProjectDTO projectDTO, int companyId)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/projects", projectDTO);
            response.EnsureSuccessStatusCode();

            ProjectDTO? createdDTO = await response.Content.ReadFromJsonAsync<ProjectDTO>();

            return createdDTO!;
        }

        public async Task ArchiveProjectAsync(int projectId, int companyId)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/projects/{projectId}/archive", companyId);
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<ProjectDTO>> GetAllProjectsAsync(int companyId)
        {
            IEnumerable<ProjectDTO> response = (await _httpClient.GetFromJsonAsync<IEnumerable<ProjectDTO>>($"api/projects?companyId={companyId}"))!;

            return response;
        }

        public async Task<IEnumerable<ProjectDTO>> GetArchivedProjects(int companyId)
        {
            IEnumerable<ProjectDTO> response = (await _httpClient.GetFromJsonAsync<IEnumerable<ProjectDTO>>($"api/projects/archived?companyId={companyId}"))!;

            return response;
        }

        public async Task<ProjectDTO?> GetProjectByIdAsync(int projectId, int companyId)
        {
            ProjectDTO? project = await _httpClient.GetFromJsonAsync<ProjectDTO>($"api/projects/{projectId}");
            return project;
        }

        public async Task RestoreProjectAsync(int projectId, int companyId)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/projects/{projectId}/restore", companyId);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateProjectAsync(ProjectDTO projectDTO, int companyId)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/projects/{projectDTO.Id}", projectDTO);
            response.EnsureSuccessStatusCode();
        }
    }
}
