using BugTrackerBC.Client.Models;
using BugTrackerBC.Data;
using BugTrackerBC.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerBC.Data
{
    public class TicketComment
    {

        private DateTimeOffset _created;
        public int Id { get; set; }

        [Required]
        public string? Content { get; set; }

        public DateTimeOffset Created
        {
            get => _created.ToLocalTime();
            set => _created = value.ToUniversalTime();
        }

        // Relationships
        public int TicketId { get; set; }
        public virtual Ticket? Ticket { get; set; }

        [Required]
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }

    public static class TicketCommentExtensions
    {
        public static TicketCommentDTO ToDTO(this TicketComment comment)
        {
            TicketCommentDTO dto = new TicketCommentDTO()
            {
                Id = comment.Id,
                Content = comment.Content,
                Created = comment.Created,
                TicketId = comment.TicketId,
                UserId = comment.User?.Id,
                User = comment.User?.ToDTO()
            };
            return dto;
        }
        
    }
}
