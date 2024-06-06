using BugTrackerBC.Data;
using BugTrackerBC.Models;
using BugTrackerBC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BugTrackerBC.Services
{
    public class CompanyRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IServiceProvider svcProvider) : ICompanyRepository
    {

        public async Task AddUserToRoleAsync(string userId, string roleName, string adminId)
        {
            //nobody can change their own roles, so dont let them
            if (userId == adminId) return;

            using IServiceScope scope = svcProvider.CreateScope();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //get the user trying to change someone's role
            ApplicationUser? admin = await userManager.FindByIdAsync(adminId);

            //verify theyre admin

            if (admin is not null && await userManager.IsInRoleAsync(admin, nameof(Roles.Admin)))
            {
                //get the user that theyre trying to change
                ApplicationUser? user = await userManager.FindByIdAsync(userId);

                //verify they belong to same company
                if (user is not null && user.CompanyId == admin.CompanyId)
                {
                    IList<string> currentRoles = await userManager.GetRolesAsync(user);
                    string? currentRole = currentRoles.FirstOrDefault(r => r != nameof(Roles.DemoUser));

                    //if user is already in role dont do anything
                    if (string.Equals(currentRole, roleName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    //users should only have one role at a time, so remove their current role
                    if (!string.IsNullOrEmpty(currentRole))
                    {
                        await userManager.RemoveFromRoleAsync(user, currentRole);
                    }

                    //add  the new role

                    await userManager.AddToRoleAsync(user, roleName);
                }
            }
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

        public async Task UpdateCompanyAsync(Company company, string adminId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            //create a UserManager for this method, similar to creating a dbContext

            using IServiceScope scope = svcProvider.CreateScope();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //get the user trying to change someone's role
            ApplicationUser? admin = await userManager.FindByIdAsync(adminId);

            //verify theyre admin

            if (admin is not null && await userManager.IsInRoleAsync(admin, nameof(Roles.Admin)))
            {
                bool shouldEdit = await context.Companies.AnyAsync(c => c.Id == company.Id && admin.CompanyId == company.Id);
                if (shouldEdit == true)
                {

                    FileUpload? oldImage = null;
                    if (company.Image != null)
                    {
                        context.Files.Add(company.Image); //add images to db table

                        oldImage = await context.Files.FirstOrDefaultAsync(f => f.Id == company.ImageId); //checks for old image

                        company.ImageId = company.Image.Id; //fix the foreign key
                    }

                    context.Companies.Update(company);
                    await context.SaveChangesAsync();

                    if (oldImage != null)
                    {
                        context.Files.Remove(oldImage);


                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    Console.WriteLine("Failed to update, company may be null");
                }
                }
            }



        }
    }
