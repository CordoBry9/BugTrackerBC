using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using System.Net.Http.Json;

namespace BugTrackerBC.Client.Services
{
    public class WASMCompanyDTOService : ICompanyDTOService
    {

        private readonly HttpClient _httpClient;

        public WASMCompanyDTOService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CompanyDTO?> GetCompanyByIdAsync(int companyId)
        {
            CompanyDTO company = (await _httpClient.GetFromJsonAsync<CompanyDTO>($"api/companies/{companyId}"))!;
            
            return company;
        }

        public async Task<IEnumerable<UserDTO>> GetCompanyMembersAsync(int companyId)
        {
            IEnumerable<UserDTO> members = (await _httpClient.GetFromJsonAsync<IEnumerable<UserDTO>>($"/api/companies/members"))!;
            return members;
        }

        public async Task<string> GetUserRoleAsync(string userId, int companyId)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/companies/members/{userId}/role");
            response.EnsureSuccessStatusCode();

            string role = await response.Content.ReadAsStringAsync();
            return role!;
        }

        public async Task<IEnumerable<UserDTO>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            IEnumerable<UserDTO> UsersInRole = [];

            UsersInRole = await _httpClient.GetFromJsonAsync<IEnumerable<UserDTO>>($"api/companies/{roleName}/users") ?? [];
            return UsersInRole;

        }

        public async Task UpdateCompanyAsync(CompanyDTO company, string adminId)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/companies/update/{company.Id}", company);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateUserRoleAsync(UserDTO user, string adminId)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/companies/update/member", user);
            response.EnsureSuccessStatusCode();
        }
    }
}
