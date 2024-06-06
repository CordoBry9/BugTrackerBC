using BugTrackerBC.Client.Models;
using BugTrackerBC.Helpers.Extensions;
using BugTrackerBC.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerBC.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} at most {1} characters long.", MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} at most {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [NotMapped]
        public string? FullName => $"{FirstName} {LastName}";

        public Guid? ImageId { get; set; }

        public virtual FileUpload? Image { get; set; }

        public int CompanyId { get; set; }
        public virtual Company? Company { get; set; }
        public virtual ICollection<Project>? Projects { get; set; }

    }
    public static class ApplicationUserExtensions
    {
        public static UserDTO ToDTO(this ApplicationUser user)
        {
            UserDTO dto = new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ImageUrl = user.ImageId != null ? $"api/Uploads/{user.ImageId}" : UploadHelper.DefaultProfilePicture,
            };

            //if (user.Projects != null)
            //{
            //    foreach (Project project in user.Projects)
            //    {
            //        ProjectDTO projectDTO = project.ToDTO();
            //        dto.Projects.Add(projectDTO);
            //    }
            //}

            return dto;
        }

    }
}
