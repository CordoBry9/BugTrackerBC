
using BugTrackerBC.Data;
using BugTrackerBC.Models;

namespace BugTrackerBC.Services.Interfaces
{
    public interface ICompanyRepository
    {
        Task<Company?> GetCompanyByIdAsync(int id);

        Task<string> GetUserRoleAsync(string userId, int id);

        Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string roleName, int companyId);

        Task AddUserToRoleAsync(string userId, string roleName, string adminId);

        Task UpdateCompanyAsync(Company company, string adminId);




    }
}
