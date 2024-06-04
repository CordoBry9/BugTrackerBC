using BugTrackerBC.Client.Components.Pages.ProjectPages;
using BugTrackerBC.Client.Models;
using BugTrackerBC.Data;
using BugTrackerBC.Models;
using BugTrackerBC.Services.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.ComponentModel.Design;

namespace BugTrackerBC.Services
{
    public class TicketRepository : ITicketRepository
    {

        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public TicketRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }


        public async Task<Ticket> AddTicketAsync(Ticket ticket, int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

                    ticket.Created = DateTime.UtcNow;
                    context.Tickets.Add(ticket);
                    await context.SaveChangesAsync();
                    return ticket;

        }


        public async Task<IEnumerable<Ticket>> GetAllTicketsAsync(int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

            IEnumerable<Ticket> tickets = await context.Tickets
                                        .Include(t => t.Project)
                                        .Where(t => t.Project!.CompanyId == companyId)
                                        .ToListAsync();

            return tickets;
        }

        public async Task<Ticket?> GetTicketByIdAsync(int ticketId, int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

            Ticket? ticket = await context.Tickets.Include(t => t.SubmitterUser)
                                       .Include(t => t.DeveloperUser)
                                       .Include(t => t.Attachments)
                                       .ThenInclude(a => a.FileUpload)
                                       .Include(t => t.Comments)
                                       .Include(t => t.Project)
                                       .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId);

            return ticket;
        }
        public async Task ArchiveTicketAsync(int ticketId, int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

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
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

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

        public async Task<Ticket> UpdateTicketAsync(Ticket ticket, int companyId, string userId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

            Ticket? existingTicket = await context.Tickets
                                              .Include(t => t.Project)
                                              .FirstOrDefaultAsync(t => t.Id == ticket.Id && t.Project!.CompanyId == companyId);

            if (existingTicket == null)
            {
                throw new Exception("Ticket not found in DB");
            }

            // Update the ticket details
            existingTicket.Title = ticket.Title;
            existingTicket.Description = ticket.Description;
            existingTicket.Priority = ticket.Priority;
            existingTicket.Updated = DateTimeOffset.UtcNow; //have this show on tables when updated
            existingTicket.Archived = ticket.Archived;
            existingTicket.ArchivedByProject = ticket.ArchivedByProject;
            existingTicket.Type = ticket.Type;
            existingTicket.Status = ticket.Status;
            existingTicket.SubmitterUserId = ticket.SubmitterUserId;
            existingTicket.DeveloperUserId = ticket.DeveloperUserId;

            context.Tickets.Update(existingTicket);
            await context.SaveChangesAsync();

            return existingTicket;
        }

        public async Task<IEnumerable<TicketComment>> GetTicketCommentsAsync(int ticketId, int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

            IEnumerable<TicketComment> ticketcomments = await context.TicketComments
                                        .Include(t => t.Ticket)
                                        .Include(t => t.User)
                                        .Where(t => t.TicketId == ticketId && t.User!.CompanyId == companyId)
                                        .ToListAsync();

            return ticketcomments;
        }

        public async Task<TicketComment?> GetCommentByIdAsync(int commentId, int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

            TicketComment? ticketcomment = await context.TicketComments
                                        .Include(c => c.User)
                                        .FirstOrDefaultAsync(c => c.Id == commentId && c.User!.CompanyId == companyId);

            return ticketcomment;
        }

        public async Task AddCommentAsync(TicketComment comment, int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

            comment.Created = DateTime.UtcNow;
            context.TicketComments.Add(comment);
            await context.SaveChangesAsync();

        }

        public async Task DeleteCommentAsync(int commentId, int companyId)
        {
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

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
            using ApplicationDbContext context = _dbContextFactory.CreateDbContext();

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
