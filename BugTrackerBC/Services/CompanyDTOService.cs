using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using BugTrackerBC.Data;
using BugTrackerBC.Helpers.Extensions;
using BugTrackerBC.Models;
using BugTrackerBC.Services.Interfaces;
using Microsoft.CodeAnalysis;

namespace BugTrackerBC.Services
{
    public class CompanyDTOService : ICompanyDTOService
    {
        private readonly ICompanyRepository _repository;

        public CompanyDTOService(ICompanyRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UserDTO>> GetCompanyMembersAsync(int companyId)
        {
            Company? company = await _repository.GetCompanyByIdAsync(companyId);
            if (company == null) return [];

            List<UserDTO> members = [];

            foreach (ApplicationUser user in company.Members)
            {
                UserDTO member = user.ToDTO();
                member.Role = await _repository.GetUserRoleAsync(user.Id, companyId);
                members.Add(member);
            }

            return members;
        }
        public async Task<string> GetUserRoleAsync(string userId, int companyId)
        {
            string? role = await _repository.GetUserRoleAsync(userId, companyId);

            return role;

        }

        public async Task<CompanyDTO?> GetCompanyByIdAsync(int companyId)
        {
            Company? company = await _repository.GetCompanyByIdAsync(companyId);

            return company?.ToDTO();
        }

        public async Task<IEnumerable<UserDTO>> GetUsersInRoleAsync(string roleName, int companyId)
        {

            //get users in role
            IEnumerable<ApplicationUser> users = await _repository.GetUsersInRoleAsync(roleName, companyId);
            // make then dtos
            IEnumerable<UserDTO> userDTOs = users.Select(u => u.ToDTO());

            //dont have to look up their role, we already know what it is
            foreach (UserDTO user in userDTOs)
            {
                // assing role

                user.Role = roleName;
            }

            return userDTOs;
        }

        public async Task UpdateUserRoleAsync(UserDTO user, string adminId)
        {
            if (string.IsNullOrEmpty(user.Role)) return;

            await _repository.AddUserToRoleAsync(user.Id!, user.Role, adminId);
        }

        public async Task UpdateCompanyAsync(CompanyDTO company, string adminId)
        {
            Company? updateCompany = await _repository.GetCompanyByIdAsync(company.Id);

            if(updateCompany != null)
            {
                updateCompany.Name = company.Name;
                updateCompany.Description = company.Description;

                if (company.ImageUrl!.StartsWith("data:"))
                {
                    updateCompany.Image = UploadHelper.GetFileUpload(company.ImageUrl);
                }
                else
                {
                    updateCompany.Image = null;
                }
                await _repository.UpdateCompanyAsync(updateCompany, adminId);
            }

        }
    }
}
