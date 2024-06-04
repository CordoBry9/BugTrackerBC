using BugTrackerBC.Client.Models;
using BugTrackerBC.Data;
using BugTrackerBC.Helpers.Extensions;
using Microsoft.AspNetCore.Connections;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerBC.Models
{
    public class Project
    {

        private DateTimeOffset _created;
        private DateTimeOffset _startDate;
        private DateTimeOffset _endDate;

        public int Id {  get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; }

        public DateTimeOffset Created
        {
            get => _created.ToLocalTime();
            set => _created = value.ToUniversalTime();
        }
        public DateTimeOffset StartDate
        {
            get => _startDate.ToLocalTime();
            set => _startDate = value.ToUniversalTime();
        }

        public DateTimeOffset EndDate
        {
            get => _endDate.ToLocalTime();
            set => _endDate = value.ToUniversalTime();
        }

        public ProjectPriority Priority { get; set; }

        public bool Archived { get; set; }

        //relationships
        [Required]
        public int CompanyId { get; set; }
        public virtual Company? Company { get; set; }

        public virtual ICollection<ApplicationUser> Members { get; set; } = new HashSet<ApplicationUser>();

        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }

    public static class ProjectExtensions
    {
        public static ProjectDTO ToDTO (this Project project)
        {
            ProjectDTO dto = new ProjectDTO
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Created = project.Created,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Priority = project.Priority,
                Archived = project.Archived,
                
            };


            foreach(ApplicationUser member in project.Members)
            {
                UserDTO memberDTO = member.ToDTO();
                dto.Members.Add(memberDTO);
            }

            foreach(Ticket ticket in project.Tickets)
            {
                TicketDTO ticketDTO = ticket.ToDTO();
                dto.Tickets.Add(ticketDTO);
            }

            return dto;
        }
    }
}
