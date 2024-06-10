using BugTrackerBC.Client.Components.Pages.ProjectPages;
using BugTrackerBC.Client.Models;
using BugTrackerBC.Data;
using BugTrackerBC.Models;
using BugTrackerBC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.ComponentModel.Design;

namespace BugTrackerBC.Services
{
    public class TicketRepository(IDbContextFactory<ApplicationDbContext> contextFactory, IServiceProvider svcProvider) : ITicketRepository
    {


        public async Task<Ticket> AddTicketAsync(Ticket ticket, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            ticket.Created = DateTime.UtcNow;
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
            return ticket;

        }


        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync(int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            IEnumerable<Ticket> tickets = await context.Tickets
                                        .Include(t => t.Project)
                                        .Where(t => t.Project!.CompanyId == companyId)
                                        .ToListAsync();

            return tickets;
        }
        public async Task<Ticket?> GetTicketByIdAsync(int ticketId, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            Ticket? ticket = await context.Tickets
                .Include(t => t.SubmitterUser)
                .Include(t => t.DeveloperUser)
                .Include(t => t.Comments)
                .Include(t => t.Project)
                .Include(t => t.Attachments)
                    .ThenInclude(a => a.FileUpload)
                .Include(t => t.Attachments)
                    .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId);

            return ticket;
        }



        public async Task ArchiveTicketAsync(int ticketId, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            Ticket? ticket = await context.Tickets.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId);

            if (ticket != null)
            {
                ticket.Archived = true;
                context.Tickets.Update(ticket);
                await context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Failed to archive, ticket may be null");
            }
        }

        public async Task RestoreTicketAsync(int ticketId, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            Ticket? ticket = await context.Tickets.Include(t => t.Project)
                                       .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId);

            if (ticket != null)
            {
                try
                {
                    ticket.Archived = false;
                    context.Tickets.Update(ticket);
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

        public async Task UpdateTicketAsync(Ticket ticket, int companyId, string userId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            bool shouldUpdate = await context.Tickets
                                              .Include(t => t.Project)
                                              .Include(t => t.DeveloperUser)
                                              .Include(t => t.SubmitterUser)
                                              .AnyAsync(t => t.Id == ticket.Id && t.Project!.CompanyId == companyId);

            if (shouldUpdate)
            {
                context.Tickets.Update(ticket);
                await context.SaveChangesAsync();
            }


        }

        public async Task<IEnumerable<TicketComment>> GetTicketCommentsAsync(int ticketId, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            IEnumerable<TicketComment> ticketcomments = await context.TicketComments
                                        .Include(t => t.Ticket)
                                        .Include(t => t.User)
                                        .Where(t => t.TicketId == ticketId && t.User!.CompanyId == companyId)
                                        .ToListAsync();

            return ticketcomments;
        }

        public async Task<TicketComment?> GetCommentByIdAsync(int commentId, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            TicketComment? ticketcomment = await context.TicketComments
                                        .Include(c => c.User)
                                        .FirstOrDefaultAsync(c => c.Id == commentId && c.User!.CompanyId == companyId);

            return ticketcomment;
        }

        public async Task AddCommentAsync(TicketComment comment, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            comment.Created = DateTime.UtcNow;
            context.TicketComments.Add(comment);
            await context.SaveChangesAsync();

        }

        public async Task DeleteCommentAsync(int commentId, int companyId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            TicketComment? ticketcomment = await context.TicketComments
                                         .Include(c => c.User)
                                         .FirstOrDefaultAsync(c => c.Id == commentId && c.User!.CompanyId == companyId);

            if (ticketcomment != null)
            {
                context.TicketComments.Remove(ticketcomment);
                await context.SaveChangesAsync();
            }
        }

        public async Task UpdateCommentAsync(TicketComment comment, string userId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            TicketComment? ticketcomment = await context.TicketComments
                                            .Include(c => c.User)
                                            .FirstOrDefaultAsync(c => c.Id == comment.Id && c.UserId == userId);

            if (ticketcomment != null)
            {

                ticketcomment.Content = comment.Content;
                context.TicketComments.Update(ticketcomment);
                await context.SaveChangesAsync();
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

        public async Task<IEnumerable<Ticket>> GetMemberTicketsAsync(int companyId, string userId)
        {
            using ApplicationDbContext context = contextFactory.CreateDbContext();

            using IServiceScope scope = svcProvider.CreateScope();
            UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            IEnumerable<Ticket> tickets = [];

            ApplicationUser? user = await userManager.FindByIdAsync(userId);
            if (user == null) return tickets;

            bool isPM = user is not null && await userManager.IsInRoleAsync(user, nameof(Roles.ProjectManager));

            if (isPM)
            {
                tickets = await context.Tickets
                        .Where(t => t.Project!.CompanyId == companyId && t.Archived == false && t.Project.Members.Any(m => m.Id == userId) || t.SubmitterUserId == userId)
                        .Include(t => t.Project)
                        .Include(t => t.Comments)
                        .Include(t => t.SubmitterUser)
                        .Include(t => t.DeveloperUser)
                        .OrderByDescending(t => t.Created)
                        .ToListAsync();

                return tickets;
            }

            bool isDev = user is not null && await userManager.IsInRoleAsync(user, nameof(Roles.Developer));

            if (isDev)
            {

                tickets = await context.Tickets
                       .Where(t => t.Project!.CompanyId == companyId && t.Archived == false && t.SubmitterUserId == userId || t.DeveloperUserId == userId)
                       .Include(t => t.Project)
                       .Include(t => t.Comments)
                       .Include(t => t.SubmitterUser)
                       .Include(t => t.DeveloperUser)
                       .OrderByDescending(t => t.Created)
                       .ToListAsync();

                return tickets;

            }


            bool isSubmitter = user is not null && await userManager.IsInRoleAsync(user, nameof(Roles.Submitter));

            if (isDev)
            {

                tickets = await context.Tickets
                       .Where(t => t.Project!.CompanyId == companyId && t.Archived == false && t.SubmitterUserId == userId)
                       .Include(t => t.Project)
                       .Include(t => t.Comments)
                       .Include(t => t.SubmitterUser)
                       .Include(t => t.DeveloperUser)
                       .OrderByDescending(t => t.Created)
                       .ToListAsync();

                return tickets;

            }

            return tickets;
        }
    }
}
