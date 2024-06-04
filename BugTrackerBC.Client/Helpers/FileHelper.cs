using Microsoft.AspNetCore.Components.Forms;

namespace BugTrackerBC.Client.Helpers
{
    public class FileHelper
    {

        public static readonly string DefaultCategoryPicture = "/img/default_category.png";
        public static readonly string DefaultProfilePicture = "/img/Default_pfp.svg";
        public static readonly string DefaultCompanyPicture = "/img/penandpaper.jpg";
        public static readonly int MaxFileSize = 5 * 1024 * 1024;

        public static async Task<string> GetDataUrl(IBrowserFile file)
        {
            using Stream fileStream = file.OpenReadStream(MaxFileSize);
            using MemoryStream ms = new MemoryStream();
            await fileStream.CopyToAsync(ms);

            byte[] imageBytes = ms.ToArray();
            string base64String = Convert.ToBase64String(imageBytes);
            string dataUrl = $"data:{file.ContentType};base64, {base64String}";

            return dataUrl;
        }
    }
}
