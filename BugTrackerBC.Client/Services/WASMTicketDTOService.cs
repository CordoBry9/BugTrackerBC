using BugTrackerBC.Client.Components.UI.TicketsUI;
using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace BugTrackerBC.Client.Services
{
    public class WASMTicketDTOService : ITicketDTOService
    {
        private readonly HttpClient _httpClient;

        public WASMTicketDTOService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TicketDTO> AddTicketAsync(TicketDTO ticketDTO, int companyId)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/tickets", ticketDTO);
            response.EnsureSuccessStatusCode();

            TicketDTO? createdDTO = await response.Content.ReadFromJsonAsync<TicketDTO>();

            return createdDTO!;
        }

        public async Task ArchiveTicketAsync(int ticketId, int companyId)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/tickets/{ticketId}/archive", companyId);
            response.EnsureSuccessStatusCode();
        }


        public async Task<IEnumerable<TicketDTO>> GetAllTicketsAsync(int companyId)
        {
            IEnumerable<TicketDTO> response = (await _httpClient.GetFromJsonAsync<IEnumerable<TicketDTO>>($"api/tickets"))!;

            return response;
        }

        public async Task<TicketDTO?> GetTicketByIdAsync(int ticketId, int companyId)
        {
            TicketDTO? ticket = await _httpClient.GetFromJsonAsync<TicketDTO>($"api/tickets/{ticketId}");
            return ticket;
        }


        public async Task RestoreTicketAsync(int ticketId, int companyId)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/tickets/{ticketId}/restore", companyId);
            response.EnsureSuccessStatusCode();
        }


        public async Task UpdateTicketAsync(TicketDTO ticketDTO, int companyId, string userId)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/tickets/{ticketDTO.Id}", ticketDTO);
            response.EnsureSuccessStatusCode();
        }

        public async Task<TicketCommentDTO?> GetCommentByIdAsync(int commentId, int companyId)
        {
            TicketCommentDTO comment = (await _httpClient.GetFromJsonAsync<TicketCommentDTO>($"api/tickets/comments/{commentId}"))!;
            return comment;
        }
        public async Task<IEnumerable<TicketCommentDTO>> GetTicketCommentsAsync(int ticketId, int companyId)
        {
            IEnumerable<TicketCommentDTO> comments = (await _httpClient.GetFromJsonAsync<IEnumerable<TicketCommentDTO>>($"api/tickets/{ticketId}/comments"))!;

            return comments;
        }
        public async Task AddCommentAsync(TicketCommentDTO comment, int companyId)
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/tickets/comments", comment);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateCommentAsync(TicketCommentDTO comment, string userId)
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/tickets/comments/{comment.Id}", comment);
            response.EnsureSuccessStatusCode();
        }
        public async Task DeleteCommentAsync(int commentId, int companyId)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"api/tickets/comments/{commentId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<TicketAttachmentDTO> AddTicketAttachment(TicketAttachmentDTO attachment, byte[] uploadData, string contentType, int companyId)
        {
            using var formData = new MultipartFormDataContent();
            formData.Headers.ContentDisposition = new("form-data");

            var fileContent = new ByteArrayContent(uploadData);
            fileContent.Headers.ContentType = new(contentType);

            if (string.IsNullOrWhiteSpace(attachment.FileName))
            {
                formData.Add(fileContent, "file");
            }
            else
            {
                formData.Add(fileContent, "file", attachment.FileName);
            }

            formData.Add(new StringContent(attachment.Id.ToString()), nameof(attachment.Id));
            formData.Add(new StringContent(attachment.FileName ?? string.Empty), nameof(attachment.FileName));
            formData.Add(new StringContent(attachment.Description ?? string.Empty), nameof(attachment.Description));
            formData.Add(new StringContent(DateTimeOffset.Now.ToString()), nameof(attachment.Created));
            formData.Add(new StringContent(attachment.UserId ?? string.Empty), nameof(attachment.UserId));
            formData.Add(new StringContent(attachment.TicketId.ToString()), nameof(attachment.TicketId));

            var res = await _httpClient.PostAsync($"api/tickets/{attachment.TicketId}/attachments", formData);
            res.EnsureSuccessStatusCode();

            var addedAttachment = await res.Content.ReadFromJsonAsync<TicketAttachmentDTO>();
            return addedAttachment!;
        }

        public async Task DeleteTicketAttachment(int attachmentId, int companyId)
        {
            var res = await _httpClient.DeleteAsync($"api/tickets/attachments/{attachmentId}");
            res.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<TicketDTO>> GetMemberTicketsAsync(int companyId, string userId)
        {
            IEnumerable<TicketDTO> memberTickets = (await _httpClient.GetFromJsonAsync<IEnumerable<TicketDTO>>($"api/tickets/member/{userId}/tickets"))!;

            return memberTickets;
        }

        public async Task<TicketAttachmentDTO> GetAttachmentById(int attachmentId, int companyId)
        {
            TicketAttachmentDTO attachment = (await _httpClient.GetFromJsonAsync<TicketAttachmentDTO>($"api/ticket/attachments/attachment/{attachmentId}"))!;

            return attachment;
        }
    }
}
