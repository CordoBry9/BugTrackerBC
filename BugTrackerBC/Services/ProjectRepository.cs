using BugTrackerBC.Client.Models;
using BugTrackerBC.Data;
using BugTrackerBC.Models;
using BugTrackerBC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BugTrackerBC.Services
{
    public class ProjectRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IServiceProvider svcProvider) : IProjectRepository
    {

        public async Task<Project> AddProjectAsync(Project project, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            if (project.CompanyId == companyId)
            {

                project.Created = DateTime.UtcNow;
                context.Projects.Add(project);
                await context.SaveChangesAsync();
                return project;
            }
            else
            {
                throw new Exception("The project company ID does not match the Users company ID");
            }

        }

        public async Task ArchiveProjectAsync(int projectId, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            Project? project = await context.Projects.Include(t => t.Tickets)
                                       .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

            if (project != null)
            {
                project.Archived = true;
                context.Projects.Update(project);
                await context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Failed to archive, project may be null");
            }
        }

        public async Task<IEnumerable<Project>> GetAllProjectsAsync(int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            IEnumerable<Project> projects = await context.Projects
                                           .Where(p => p.CompanyId == companyId)
                                           .ToListAsync();

            return projects;
        }

        public async Task<IEnumerable<Project>> GetArchivedProjectsAsync(int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();
            IEnumerable<Project> archivedprojects = await context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true)
                                 .ToListAsync();

            return archivedprojects;
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();
            return await context.Projects.Include(t => t.Tickets).Include(m => m.Members)
                                .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
        }


        public async Task RestoreProjectAsync(int projectId, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            Project? project = await context.Projects.Include(t => t.Tickets)
                                       .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

            if (project != null)
            {
                try
                {
                    project.Archived = false;
                    context.Projects.Update(project);
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.Write(ex);
                }

            }
            else
            {
                Console.WriteLine("Failed to restore, project may be null");
            };
        }

        public async Task UpdateProjectAsync(Project project, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            bool shouldEdit = await context.Projects.AnyAsync(p => p.Id == project.Id && p.CompanyId == companyId);
            if (shouldEdit == true)
            {
                Project? existingProject = await context.Projects
                    .FirstOrDefaultAsync(p => p.Id == project.Id && p.CompanyId == companyId);

                if (existingProject != null)
                {
                    existingProject.Name = project.Name;
                    existingProject.Description = project.Description;
                    existingProject.Priority = project.Priority;
                    existingProject.StartDate = project.StartDate;
                    existingProject.EndDate = project.EndDate;
                    existingProject.Archived = project.Archived;

                    context.Projects.Update(existingProject);
                    await context.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine("Failed to update, project may be null");
                }

            }
        }

        public async Task<TicketAttachment> AddTicketAttachment(TicketAttachment attachment, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            // make sure the ticket exists and belongs to this company
            var ticket = await context.Tickets
                .FirstOrDefaultAsync(t => t.Id == attachment.TicketId && t.Project!.CompanyId == companyId);

            // save it if it does
            if (ticket is not null)
            {
                attachment.Created = DateTimeOffset.Now;
                context.TicketAttachments.Add(attachment);
                await context.SaveChangesAsync();

                return attachment;
            }
            else
            {
                throw new ArgumentException("Ticket not found");
            }
        }

        public async Task DeleteTicketAttachment(int attachmentId, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            var attachment = await context.TicketAttachments
                .Include(a => a.FileUpload)
                .FirstOrDefaultAsync(a => a.Id == attachmentId && a.Ticket!.Project!.CompanyId == companyId);

            if (attachment is not null)
            {
                context.Remove(attachment);
                context.Remove(attachment.FileUpload!);
                await context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<Project>> GetMemberProjectsAsync(int companyId, string memberId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            IEnumerable<Project> projects = await context.Projects
                .Where(p => p.CompanyId == companyId && p.Archived == false && p.Members.Any(m => m.Id == memberId))
                .Include(p => p.Tickets)
                .Include(p => p.Members).ToListAsync();

            return projects;
        }

        public async Task<IEnumerable<Project>> GetMemberArchivedProjectsAsync(int companyId, string memberId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            IEnumerable<Project> projects = await context.Projects
                .Where(p => p.CompanyId == companyId && p.Archived == true && p.Members.Any(m => m.Id == memberId))
                .Include(p => p.Tickets)
                .Include(p => p.Members).ToListAsync();

            return projects;
        }

        public async Task<IEnumerable<ApplicationUser>> GetProjectMembersAsync(int projectId, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            Project? project = await context.Projects.Include(m => m.Members)
                .FirstOrDefaultAsync(m => m.Id == projectId && m.CompanyId == companyId);

            return project?.Members ?? [];
        }

        public async Task<ApplicationUser?> GetProjectManagerAsync(int projectId, int companyId)
        {
            IEnumerable<ApplicationUser> projectMembers = await GetProjectMembersAsync(projectId, companyId);

            using IServiceScope scope = svcProvider.CreateScope();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (ApplicationUser member in projectMembers)
            {
                bool isProjectManager = await userManager.IsInRoleAsync(member, nameof(Roles.ProjectManager));

                if (isProjectManager == true)
                {
                    //exit the loop early and return the project manager
                    return member;

                    //no need to loop through any more members, since theres only one PM per project
                    // and we just found it
                }
            }
            return null; //if not found
        }

        public async Task AddMemberToProjectAsync(int projectId, string userId, string managerId)
        {

            using ApplicationDbContext context = contextFactory.CreateDbContext();
            using IServiceScope scope = svcProvider.CreateScope();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // get the user trying to add a member to a project

            ApplicationUser? manager = await userManager.FindByIdAsync(managerId);

            if (manager is null) return; // if we can't find them, we can't do anything

            // if they're an admin, they can do whatever they like

            bool isAdmin = await userManager.IsInRoleAsync(manager, nameof(Roles.Admin));

            // if they're not an admin, they must be the assigned PM of this project
            if (isAdmin == false)
            {
                ApplicationUser? projectManager = await GetProjectManagerAsync(projectId, manager.CompanyId);
                // they're not an admin nor are they the assigned PM for this project, so don't let them add members 
                if (projectManager?.Id != managerId) return;
            }

            // Look for a user with the given ID in the same company as the manager

            ApplicationUser? userToAdd = await context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == manager.CompanyId);
            if (userToAdd is null) return;

            // if the new member is a PM, they need to be added using AssignProject ManagerAsync instead 

            bool userIsProjectManager = await userManager.IsInRoleAsync(userToAdd, nameof(Roles.ProjectManager));
            if (userIsProjectManager) return;

            // admins are never assigned projects, so don't add them if they're an admin
            bool userIsAdmin = await userManager.IsInRoleAsync(userToAdd, nameof(Roles.Admin));
            if (userIsAdmin) return;

            // we've checked the user, so now we can check the project
            Project? project = await context.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == manager.CompanyId);

            // quit if we don't find a project
            if (project is null) return;

            // if the user isn't already a member, add them
            if (project.Members.Any(m => m.Id == userToAdd.Id) == false)
            {
                project.Members.Add(userToAdd);
                await context.SaveChangesAsync();
            }


        }

        public async Task RemoveMemberFromProjectAsync(int projectId, string userId, string managerId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();
            using IServiceScope scope = svcProvider.CreateScope();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            //look up the user who is trying to remove a member

            ApplicationUser? manager = await userManager.FindByIdAsync(managerId);
            if (manager == null) return;

            //look up the project by ID and ensure it has the same CompanyId
            Project? project = await context.Projects.Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == manager.CompanyId);

            if (project == null) return;

            //look for the user to remove from the project's members

            ApplicationUser? memberToRemove = project.Members.FirstOrDefault(m => m.Id == userId);
            if (memberToRemove == null) return;

            //finally, remove the requested member and save our changes

            project.Members.Remove(memberToRemove);
            await context.SaveChangesAsync();
        }



        public async Task AssignProjectManagerAsync(int projectId, string userId, string adminId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            using IServiceScope scope = svcProvider.CreateScope();

            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // get the user attempting to assign a PM
            ApplicationUser? admin = await context.Users.FindAsync(adminId);

            if (admin is null) return;

            bool isAdmin = admin is not null && await userManager.IsInRoleAsync(admin, nameof(Roles.Admin));

            bool isPM = admin is not null && await userManager.IsInRoleAsync(admin, nameof(Roles.ProjectManager));

            // if they're an admin OR a PM assigning themselves a project, continue
            // (e.g. a PM creating a new project should be assigned to it by default)
            if (isAdmin == true || (isPM == true && userId == adminId))
            {
                ApplicationUser? projectManager = await context.Users.FindAsync(userId);

                //make sure the user beng assigned is a pm in the same company
                if (projectManager is not null && projectManager.CompanyId == admin!.CompanyId
                    && await userManager.IsInRoleAsync(projectManager, nameof(Roles.ProjectManager)))
                {
                    //remove any existing PM, since there can only be one
                    await RemoveProjectManagerAsync(projectId, adminId);

                    Project? project = await context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId
                    && p.CompanyId == admin.CompanyId);

                    //finally, add this user as PM if the project exists
                    if (project is not null)
                    {
                        project.Members.Add(projectManager);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId, string adminId)
        {
            using IServiceScope scope = svcProvider.CreateScope();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? admin = await userManager.FindByIdAsync(adminId);
            if (admin is null) return;

            ApplicationUser? projectManager = await GetProjectManagerAsync(projectId, admin.CompanyId);

            //nothing to do if there isn't a pm
            if (projectManager is null) return;

            // if there is a PM, make sure the user attempting to remove them is an an admin
            if (await userManager.IsInRoleAsync(admin, nameof(Roles.Admin)))
            {
                await RemoveMemberFromProjectAsync(projectId, projectManager.Id, adminId);
            }

        }

    }
}
