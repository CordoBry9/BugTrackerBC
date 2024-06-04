using BugTrackerBC.Data;
using BugTrackerBC.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerBC.Client.Models 
{
    public class TicketAttachment
    {

        private DateTimeOffset _created;
        public int Id { get; set; }

        [Required]
        public string? FileName { get; set; }

        public string? Description { get; set; }

        public DateTimeOffset Created
        {
            get => _created.ToLocalTime();
            set => _created = value.ToUniversalTime();
        }

        //relationships
        public Guid FileUploadId { get; set; }
        public virtual FileUpload? FileUpload { get; set; }

        [Required]
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public int TicketId { get; set; }
        public virtual Ticket? Ticket { get; set; }


    }
    public static class TicketAttachmentExtensions
    {
        public static TicketAttachmentDTO ToDTO(this TicketAttachment attachment)
        {
            TicketAttachmentDTO dto = new TicketAttachmentDTO()
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                Description = attachment.Description,
                Created = attachment.Created,
                AttachmentUrl = $"api/Uploads/{attachment.FileUploadId}",
                UserId = attachment.UserId,
                User = attachment.User?.ToDTO(),
                TicketId = attachment.TicketId
            };

            return dto;
        }
    }

}


