using BugTrackerBC.Client.Models;
using BugTrackerBC.Data;
using BugTrackerBC.Models;

namespace BugTrackerBC.Services.Interfaces
{

    public interface ITicketRepository
    {

        Task<IEnumerable<Ticket>> GetAllTicketsAsync(int companyId);
        Task<Ticket?> GetTicketByIdAsync(int ticketId, int companyId);
        Task<IEnumerable<Ticket>> GetMemberTicketsAsync(int companyId, string userId);
        Task<Ticket> AddTicketAsync(Ticket ticket, int companyId);
        // you will not need userId at this point, but put it in the method anyways for future authorization features O references
        Task UpdateTicketAsync(Ticket ticket, int companyId, string userId);

        Task ArchiveTicketAsync(int ticketId, int companyId);

        Task RestoreTicketAsync(int ticketId, int companyId);

        Task<IEnumerable<TicketComment>> GetTicketCommentsAsync(int ticketId, int companyId);

        Task<TicketComment?> GetCommentByIdAsync(int commentId, int companyId);

        Task AddCommentAsync(TicketComment comment, int companyId);

        Task DeleteCommentAsync(int commentId, int companyId);

        Task UpdateCommentAsync(TicketComment comment, string userId);

        #region Attachments
        Task<TicketAttachment> AddTicketAttachment(TicketAttachment attachment, int companyId);

        Task<TicketAttachment> GetAttachmentById(int attachmentId, int companyId);

        Task DeleteTicketAttachment(int attachmentId, int companyId);
        #endregion
    }
}
