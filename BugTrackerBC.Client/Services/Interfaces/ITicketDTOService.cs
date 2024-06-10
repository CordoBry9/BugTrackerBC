using BugTrackerBC.Client.Models;

namespace BugTrackerBC.Client.Services.Interfaces
{
    public interface ITicketDTOService
    {
        Task<IEnumerable<TicketDTO>> GetAllTicketsAsync(int companyId);

        Task<TicketDTO?> GetTicketByIdAsync(int ticketId, int companyId);

        Task<IEnumerable<TicketDTO>> GetMemberTicketsAsync(int companyId, string userId);

        Task<TicketDTO> AddTicketAsync(TicketDTO ticket, int companyId);

        Task UpdateTicketAsync(TicketDTO ticket, int companyId, string userId);

        Task ArchiveTicketAsync(int ticketId, int companyId);

        Task RestoreTicketAsync(int ticketId, int companyId);

        #region Ticket Comments

        Task <IEnumerable<TicketCommentDTO>> GetTicketCommentsAsync(int ticketId, int companyId);

        Task<TicketCommentDTO?> GetCommentByIdAsync(int commentId, int companyId);

        Task AddCommentAsync(TicketCommentDTO comment, int companyId);

        Task DeleteCommentAsync(int commentId, int companyId);

        Task UpdateCommentAsync(TicketCommentDTO comment, string userId);

        #endregion

        #region Attachments
        Task<TicketAttachmentDTO> AddTicketAttachment(TicketAttachmentDTO attachment, byte[] uploadData, string contentType, int companyId);
        Task DeleteTicketAttachment(int attachmentId, int companyId);

        #endregion
    }
}
