using BugTrackerBC.Client.Helpers;
using BugTrackerBC.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.RegularExpressions;

namespace BugTrackerBC.Helpers.Extensions
{
    public static class UploadHelper
    {
        public static readonly string DefaultProfilePicture = FileHelper.DefaultProfilePicture;
        public static readonly string DefaultCompanyPicture = FileHelper.DefaultCompanyPicture;
        public static readonly string DefaultCategoryPicture = FileHelper.DefaultCategoryPicture;
        public static readonly int MaxFileSize = FileHelper.MaxFileSize;

        public static async Task<FileUpload> GetFileUploadAsync(IFormFile file)
        {
            using var ms = new MemoryStream();

            await file.CopyToAsync(ms);
            byte[] data = ms.ToArray();

            if (ms.Length > 5 * 1024 * 1024)
            {
                throw new IOException("Images must be 5MB or less");
            }

            FileUpload upload = new FileUpload()
            {

                Id = Guid.NewGuid(),
                Data = data,
                Type = file.ContentType,
            };

            return upload;
        }

        public static FileUpload GetFileUpload(string dataUrl)
        {
            GroupCollection matchGroups = Regex.Match(dataUrl, @"data:(?<type>.+?);base64,(?<data>.+)").Groups;

            if (matchGroups.ContainsKey("type") && matchGroups.ContainsKey("data"))
            {
                string contentType = matchGroups["type"].Value;

                byte[] data = Convert.FromBase64String(matchGroups["data"].Value);

                if (data.Length <= MaxFileSize)
                {
                    FileUpload upload = new FileUpload()
                    {
                        Id = Guid.NewGuid(),
                        Data = data,
                        Type = contentType,
                    };

                    return upload;
                }
            }

            throw new IOException("Data URL was either invalid or too large");
        }
    }
}
