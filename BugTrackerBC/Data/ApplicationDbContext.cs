using BugTrackerBC.Client.Models;
using BugTrackerBC.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerBC.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public virtual DbSet<FileUpload> Files { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Invite> Invites { get; set; } 
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<TicketAttachment> TicketAttachments { get; set; }
        public virtual DbSet<TicketComment> TicketComments { get; set; }

    }
}
