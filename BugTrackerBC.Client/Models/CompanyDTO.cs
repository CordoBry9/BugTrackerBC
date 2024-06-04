using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerBC.Client.Models
{
    public class CompanyDTO
    {

        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public virtual ICollection<ProjectDTO> Projects { get; set; } = new HashSet<ProjectDTO>();

        public virtual ICollection<UserDTO> Members { get; set; } = new HashSet<UserDTO>();

        public virtual ICollection<InviteDTO> Invites { get; set; } = new HashSet<InviteDTO>();
    }
}
