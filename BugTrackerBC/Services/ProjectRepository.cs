using BugTrackerBC.Client.Models;
using BugTrackerBC.Data;
using BugTrackerBC.Models;
using BugTrackerBC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BugTrackerBC.Services
{
    public class ProjectRepository : IProjectRepository
    {

        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public ProjectRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<Project> AddProjectAsync(Project project, int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

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
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

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
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

            IEnumerable<Project> projects = await context.Projects
                                           .Where(p => p.CompanyId == companyId)
                                           .ToListAsync();

            return projects;
        }

        public async Task<IEnumerable<Project>> GetArchivedProjectsAsync(int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();
            IEnumerable<Project> archivedprojects = await context.Projects.Where(p => p.CompanyId == companyId && p.Archived)
                                 .ToListAsync();

            return archivedprojects;
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId, int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();
            return await context.Projects.Include(t => t.Tickets).Include(m => m.Members)
                                .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
        }


        public async Task RestoreProjectAsync(int projectId, int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

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
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

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
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

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
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

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
    }
}
