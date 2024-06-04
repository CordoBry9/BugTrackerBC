using System.ComponentModel.DataAnnotations;

namespace BugTrackerBC.Models
{
    public class FileUpload
    {
        public Guid Id { get; set; }

        [Required]
        public byte[]? Data { get; set; }

        [Required]
        public string? Type { get; set; }

    }
}
