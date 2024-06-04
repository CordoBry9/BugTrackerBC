using BugTrackerBC.Data;
using BugTrackerBC.Models;
using BugTrackerBC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerBC.Services
{
    public class CompanyRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IServiceProvider svcProvider) : ICompanyRepository
    {

        public Task AddUserToRoleAsync(string userId, string roleName, string adminId)
        {
            throw new NotImplementedException();
        }

        public async Task<Company?> GetCompanyByIdAsync(int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            Company? company = await context.Companies.Include(c => c.Invites).Include(c => c.Projects).Include(c => c.Members).FirstOrDefaultAsync(c => c.Id == companyId);

            return company;
        }

        public async Task<string> GetUserRoleAsync(string userId, int companyId)
        {
            //create a UserManager for this method, similar to creating a dbContext

            using IServiceScope scope = svcProvider.CreateScope();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //find the user
            ApplicationUser? user = await userManager.FindByIdAsync(userId);

            string role = "Unknown";

            // make sure the user belongs to the company

            if (user?.CompanyId == companyId)
            {
                //get their roles
                IList<string> roles = await userManager.GetRolesAsync(user);

                //some users have their assigned role and a DemoUser role, but we dont want to show that,
                //so look up their first role that isn't DemoUser

                role = roles.FirstOrDefault(r => r != nameof(Roles.DemoUser), role);
            }

            return role;
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            using IServiceScope scope = svcProvider.CreateScope();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            IList<ApplicationUser> users = await userManager.GetUsersInRoleAsync(roleName);
            IEnumerable<ApplicationUser> companyUsers = users.Where(u => u.CompanyId == companyId);

            return companyUsers;
        }

        public Task UpdateCompanyAsync(Company company, string adminId)
        {
            throw new NotImplementedException();
        }
    }
}
