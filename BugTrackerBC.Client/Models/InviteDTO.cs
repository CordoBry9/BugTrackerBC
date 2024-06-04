using System.ComponentModel.DataAnnotations;

namespace BugTrackerBC.Client.Models
{
    public class InviteDTO
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
            get => _updateDate?.ToLocalTime();
            set => _updateDate = value?.ToUniversalTime();
        }

        [Required]

        public string? InviteeEmail { get; set; }

        public string? InviteeFirstName { get; set; }
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }

        public bool IsValid { get; set; } //to check if invite is valid 

        // Relationships

        public int ProjectId { get; set; }
        public virtual ProjectDTO? Project { get; set; }

        [Required]
        public string? InvitorId { get; set; }
        public virtual UserDTO? Invitor { get; set; }
        public string? InviteeId { get; set; }
        public virtual UserDTO? Invitee { get; set; }

    }
}
