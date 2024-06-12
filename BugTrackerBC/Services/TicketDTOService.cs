using BugTrackerBC.Client.Components.Pages.ProjectPages;
using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using BugTrackerBC.Data;
using BugTrackerBC.Models;
using BugTrackerBC.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis;

namespace BugTrackerBC.Services
{
    public class TicketDTOService : ITicketDTOService
    {
        private readonly ITicketRepository _repository;

        public TicketDTOService(ITicketRepository repository)
        {
            _repository = repository;
        }

        public async Task<TicketDTO> AddTicketAsync(TicketDTO ticketDTO, int companyId)
        {
            Ticket newticket = new Ticket()
            {

                Title = ticketDTO.Title,
                Description = ticketDTO.Description,
                Created = DateTime.Now,
                Type = ticketDTO.Type,
                Status = TicketStatus.New,
                DeveloperUserId = ticketDTO.DeveloperUserId,
                SubmitterUserId = ticketDTO.SubmitterUserId,
                Priority = ticketDTO.Priority,
                Archived = ticketDTO.Archived,
                ArchivedByProject = ticketDTO.ArchivedByProject,
                ProjectId = ticketDTO.ProjectId 

            };

            Ticket createdTicket = await _repository.AddTicketAsync(newticket, companyId);
            return createdTicket.ToDTO();
        }


        public async Task<IEnumerable<TicketDTO>> GetAllTicketsAsync(int companyId)
        {
            IEnumerable<Ticket> tickets = await _repository.GetAllTicketsAsync(companyId);
            return tickets.Select(t => t.ToDTO()).ToList();
        }

        public async Task<TicketDTO?> GetTicketByIdAsync(int ticketId, int companyId)
        {
            Ticket? ticket = await _repository.GetTicketByIdAsync(ticketId, companyId);
            return ticket?.ToDTO();
        }
        public async Task ArchiveTicketAsync(int ticketId, int companyId)
        {
            await _repository.ArchiveTicketAsync(ticketId, companyId);
        }
        public async Task<IEnumerable<TicketDTO>> GetMemberTicketsAsync(int companyId, string userId)
        {
            IEnumerable<Ticket> memberTickets = await _repository.GetMemberTicketsAsync(companyId, userId);
            return memberTickets.Select(t => t.ToDTO()).ToList();
        }

        public async Task RestoreTicketAsync(int ticketId, int companyId)
        {
            await _repository.RestoreTicketAsync(ticketId, companyId);
        }

        public async Task UpdateTicketAsync(TicketDTO ticketDTO, int companyId, string userId)
        {
            Ticket? updatedTicket = await _repository.GetTicketByIdAsync(ticketDTO.Id, companyId);
            if(updatedTicket != null)
            {
                updatedTicket.Id = ticketDTO.Id;
                updatedTicket.Title = ticketDTO.Title;
                updatedTicket.Description = ticketDTO.Description;
                updatedTicket.Created = ticketDTO.Created;
                updatedTicket.Priority = ticketDTO.Priority;
                updatedTicket.Updated = DateTimeOffset.Now;
                updatedTicket.Archived = ticketDTO.Archived;
                updatedTicket.ArchivedByProject = ticketDTO.ArchivedByProject;
                updatedTicket.Type = ticketDTO.Type;
                updatedTicket.Status = ticketDTO.Status;
                updatedTicket.SubmitterUserId = ticketDTO.SubmitterUserId;
                updatedTicket.DeveloperUserId = ticketDTO.DeveloperUserId;
                updatedTicket.DeveloperUser = null;
                updatedTicket.SubmitterUser = null;
                updatedTicket.Project = null;

                await _repository.UpdateTicketAsync(updatedTicket, companyId, userId);

            }
        }

        public async Task<IEnumerable<TicketCommentDTO>> GetTicketCommentsAsync(int ticketId, int companyId)
        {
            IEnumerable<TicketComment> ticketcomments = await _repository.GetTicketCommentsAsync(ticketId, companyId);
            return ticketcomments.Select(t => t.ToDTO()).ToList();
        }

        public async Task<TicketCommentDTO?> GetCommentByIdAsync(int commentId, int companyId)
        {
            TicketComment ticketcomment = (await _repository.GetCommentByIdAsync(commentId, companyId))!;

            return ticketcomment.ToDTO();
        }

        public async Task AddCommentAsync(TicketCommentDTO comment, int companyId)
        {
            TicketComment ticketComment = new TicketComment
            {
                Id = comment.Id,
                Content = comment.Content,
                Created = DateTime.UtcNow,
                TicketId = comment.TicketId,
                UserId = comment.UserId,
            };

              await _repository.AddCommentAsync(ticketComment, companyId);
        }

        public async Task DeleteCommentAsync(int commentId, int companyId)
        {
            await _repository.DeleteCommentAsync(commentId, companyId);
        }

        public async Task UpdateCommentAsync(TicketCommentDTO comment, string userId)
        {
            TicketComment newcomment = new TicketComment()
            {
                Id = comment.Id,
                Content = comment.Content,
                Created = comment.Created,
                UserId = userId,
                TicketId = comment.TicketId
            };

            await _repository.UpdateCommentAsync(newcomment, userId);
        }

        public async Task<TicketAttachmentDTO> AddTicketAttachment(TicketAttachmentDTO attachment, byte[] uploadData, string contentType, int companyId)
        {
            FileUpload file = new()
            {
                Type = contentType,
                Data = uploadData,
            };

            TicketAttachment dbAttachment = new()
            {
                TicketId = attachment.TicketId,
                Description = attachment.Description,
                FileName = attachment.FileName,
                FileUpload = file,
                Created = DateTimeOffset.Now,
                UserId = attachment.UserId
            };

            dbAttachment = await _repository.AddTicketAttachment(dbAttachment, companyId);

            return dbAttachment.ToDTO();
        }

        public async Task DeleteTicketAttachment(int attachmentId, int companyId)
        {
            await _repository.DeleteTicketAttachment(attachmentId, companyId);
        }

    }
}
