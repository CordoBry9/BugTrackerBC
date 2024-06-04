using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.ComponentModel.DataAnnotations;
using BugTrackerBC.Data;
using Microsoft.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;
using BugTrackerBC.Client.Models;

namespace BugTrackerBC.Models
{
    public class Invite
    {
        private DateTimeOffset _inviteDate;
        private DateTimeOffset? _updateDate;

        public int Id { get; set; }

        [Required]
        public DateTimeOffset InviteDate
        {
            get => _inviteDate.ToLocalTime();
            set => _inviteDate = value.ToUniversalTime();
        }
        public DateTimeOffset? UpdateDate
        {
            get => _updateDate;
            set => _updateDate = value?.ToUniversalTime();
        }

        public Guid CompanyToken {  get; set; }

        [Required]
        public string? InviteeEmail { get; set; }

        [Required]
        public string? InviteeFirstName {  get; set; }

        [Required]
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }

        public bool IsValid { get; set; } //to check if invite is valid 

        // Relationships

        public int CompanyId { get; set; }
        public virtual Company? Company { get; set; }

        public int ProjectId { get; set; }
        public virtual Project? Project { get; set; }
        [Required]
        public string? InvitorId { get; set; }
        public virtual ApplicationUser? Invitor { get; set; }
        public string? InviteeId { get; set; }
        public virtual ApplicationUser? Invitee { get; set; }


    }

    namespace BugTrackerBC.Helpers
    {
        public static class InviteExtensions
        {
            public static InviteDTO ToDTO(this Invite invite)
            {
                InviteDTO dto = new InviteDTO()
                {
                    Id = invite.Id,
                    InviteDate = invite.InviteDate,
                    UpdateDate = invite.UpdateDate,
                    InviteeEmail = invite.InviteeEmail,
                    InviteeFirstName = invite.InviteeFirstName,
                    InviteeLastName = invite.InviteeLastName,
                    Message = invite.Message,
                    IsValid = invite.IsValid,
                    ProjectId = invite.ProjectId,
                    InvitorId = invite.Invitor?.Id,
                    InviteeId = invite.Invitee?.Id,
                    Invitor = invite.Invitor?.ToDTO(),
                    Invitee = invite.Invitee?.ToDTO(),
                    Project = invite.Project?.ToDTO(),

                };

                return dto;
            }
            
        }
    }

}
