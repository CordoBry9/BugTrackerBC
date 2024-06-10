using BugTrackerBC.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BugTrackerBC.Client.Models;

namespace BugTrackerBC.Models
{
    public class Ticket
    {

        private DateTimeOffset _created;
        private DateTimeOffset? _updated;

        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Description { get; set; }

        public DateTimeOffset Created
        {
            get => _created.ToLocalTime();
            set => _created = value.ToUniversalTime();
        }
        public DateTimeOffset? Updated
        {
            get => _updated?.ToLocalTime();
            set => _updated = value?.ToUniversalTime();
        }

        public bool Archived { get; set; }

        public bool ArchivedByProject { get; set; }

        public TicketPriority Priority { get; set; }

        public TicketType Type { get; set; }

        public TicketStatus Status { get; set; }

        // Relationships
        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }

        [Required]
        public string? SubmitterUserId { get; set; }

        public virtual ApplicationUser? SubmitterUser { get; set; }

        public string? DeveloperUserId { get; set; }

        public virtual ApplicationUser? DeveloperUser { get; set; }

        public virtual ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();

        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new List<TicketAttachment>();
    }

    public static class TicketExtensions
    {
        public static TicketDTO ToDTO(this Ticket ticket)
        {
            TicketDTO dto = new TicketDTO
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                Created = ticket.Created,
                Updated = ticket.Updated,
                Archived = ticket.Archived,
                ArchivedByProject = ticket.ArchivedByProject,
                Priority = ticket.Priority,
                Type = ticket.Type,
                Status = ticket.Status,
                ProjectId = ticket.ProjectId,
                SubmitterUserId = ticket.SubmitterUserId,
                DeveloperUserId = ticket.DeveloperUserId,
                DeveloperUser = ticket.DeveloperUser?.ToDTO(),
                SubmitterUser = ticket.SubmitterUser?.ToDTO(),
                //Project = ticket.Project?.ToDTO(),
            };

            foreach (TicketComment comment in ticket.Comments)
            {
                TicketCommentDTO commentDTO = comment.ToDTO();
                dto.Comments.Add(commentDTO);
            }

            foreach (TicketAttachment attachment in ticket.Attachments)
            {
                TicketAttachmentDTO attachmentDTO = attachment.ToDTO();
                dto.Attachments.Add(attachmentDTO);
            }

            return dto;
        }
    }
}
