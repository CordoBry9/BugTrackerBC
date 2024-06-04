using BugTrackerBC.Client.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerBC.Client.Models
{
    public class TicketAttachmentDTO
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

        public string? AttachmentUrl { get; set; }

        [Required]
        public string? UserId { get; set; }

        public UserDTO? User { get; set; }

        public int TicketId { get; set; }
    }
}

