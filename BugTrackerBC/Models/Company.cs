using BugTrackerBC.Client.Models;
using BugTrackerBC.Data;
using BugTrackerBC.Helpers.Extensions;
using BugTrackerBC.Models.BugTrackerBC.Helpers;
using Microsoft.AspNetCore.Connections;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerBC.Models
{
    public class Company
    {

        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public Guid? ImageId { get; set; }

        public virtual FileUpload? Image { get; set; }

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        public virtual ICollection<ApplicationUser> Members { get; set; } = new HashSet<ApplicationUser>();

        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
    }

    public static class CompanyExtensions
    {
        public static CompanyDTO ToDTO(this Company company)
        {
            CompanyDTO dto = new CompanyDTO
            {
                Id = company.Id,
                Name = company.Name,
                Description = company.Description,
                ImageUrl = company.ImageId != null ? $"api/Uploads/{company.ImageId}" : UploadHelper.DefaultCompanyPicture,
            };

            foreach (Project project in company.Projects)
            {
                ProjectDTO projectDTO = project.ToDTO();
                dto.Projects.Add(projectDTO);
            }

            foreach (ApplicationUser member in company.Members)
            {
                UserDTO memberDTO = member.ToDTO();
                dto.Members.Add(memberDTO);
            }

            foreach (Invite invite in company.Invites)
            {
                InviteDTO inviteDTO = invite.ToDTO();
                dto.Invites.Add(inviteDTO);
            }

            return dto;
        }
    }
}