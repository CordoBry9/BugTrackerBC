using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using BugTrackerBC.Data;
using BugTrackerBC.Helpers.Extensions;
using BugTrackerBC.Models;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Net.Sockets;
using System.Security.Claims;

namespace BugTrackerBC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketDTOService _ticketService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProjectDTOService _projectService;
        private int? _companyId => User.FindFirst("CompanyId") != null ? int.Parse(User.FindFirst("CompanyId")!.Value) : null;

        private string _userId => _userManager.GetUserId(User)!;
        public TicketsController(ITicketDTOService ticketService, UserManager<ApplicationUser> userManager, IProjectDTOService projectDTOService)
        {
            _ticketService = ticketService;
            _userManager = userManager;
            _projectService = projectDTOService;
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}, {nameof(Roles.Developer)}, {nameof(Roles.Submitter)}")]
        public async Task<ActionResult<TicketDTO>> AddTicket([FromBody] TicketDTO newTicket)
        {

            if (_companyId != null)
            {

                if (User.IsInRole(nameof(Roles.Admin)))
                {
                    newTicket.SubmitterUserId = _userId;

                    TicketDTO ticket = await _ticketService.AddTicketAsync(newTicket, _companyId.Value);

                    return Ok(ticket);

                }
                if (User.IsInRole(nameof(Roles.ProjectManager)))
                {
                    UserDTO? projectManager = await _projectService.GetProjectManagerAsync(newTicket.ProjectId, _companyId.Value);
                    if (projectManager.Id == _userId) 
                    {
                        newTicket.SubmitterUserId = _userId;

                        TicketDTO ticket = await _ticketService.AddTicketAsync(newTicket, _companyId.Value);

                        return Ok(ticket);
                    }
                    else
                    {

                        return BadRequest();
                    }
                }
                if (User.IsInRole(nameof(Roles.Submitter)))
                {
                    newTicket.SubmitterUserId = _userId;
                    IEnumerable<UserDTO>? members = await _projectService.GetProjectMembersAsync(newTicket.ProjectId, _companyId.Value);

                    if (members.Any(m => m.Id == newTicket.SubmitterUserId)) 
                    {
                        TicketDTO ticket = await _ticketService.AddTicketAsync(newTicket, _companyId.Value);

                        return Ok(ticket);

                    }
                    else
                    {
                        return NotFound();
                    }
                }
                if (User.IsInRole(nameof(Roles.Developer)))
                {
                    newTicket.DeveloperUserId = _userId;

                    newTicket.DeveloperUserId = _userId;
                    IEnumerable<UserDTO>? members = await _projectService.GetProjectMembersAsync(newTicket.ProjectId, _companyId.Value);

                    if (members.Any(m => m.Id == newTicket.DeveloperUserId))
                    {
                        TicketDTO ticket = await _ticketService.AddTicketAsync(newTicket, _companyId.Value);

                        return Ok(ticket);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest();
                }


            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketDTO>>> GetAllTickets()
        {
            try
            {
                if (_companyId != null)
                {
                    IEnumerable<TicketDTO> tickets = await _ticketService.GetAllTicketsAsync(_companyId.Value);
                    return Ok(tickets);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem();
            }
        }


        [HttpGet("member/{userId}/tickets")]
        public async Task<ActionResult<IEnumerable<TicketDTO>>> GetMemberTickets([FromRoute] string userId)
        {
            try
            {
                if (_companyId != null)
                {
                    IEnumerable<TicketDTO> tickets = await _ticketService.GetMemberTicketsAsync(_companyId.Value, userId);
                    return Ok(tickets);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem();
            }
        }

        [HttpGet("member/{userId}/tickets/archived")]
        public async Task<ActionResult<IEnumerable<TicketDTO>>> GetMemberArchivedTickets([FromRoute] string userId)
        {
            try
            {
                if (_companyId != null)
                {
                    IEnumerable<TicketDTO> tickets = await _ticketService.GetMemberArchivedTicketsAsync(_companyId.Value, userId);
                    return Ok(tickets);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem();
            }
        }

        [HttpGet("{ticketId:int}")]
        public async Task<ActionResult<TicketDTO?>> GetTicketById([FromRoute] int ticketId)
        {
            try
            {
                if (_companyId != null)
                {
                    TicketDTO? ticket = await _ticketService.GetTicketByIdAsync(ticketId, _companyId.Value);
                    if (ticket == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        return Ok(ticket);
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem();
            }
        }

        [HttpPut("{ticketId:int}/archive")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}")]
        public async Task<ActionResult> ArchiveTicket([FromRoute] int ticketId)
        {
            try
            {
                if (_companyId != null)
                {
                    if (User.IsInRole(nameof(Roles.Admin)))
                    {
                        await _ticketService.ArchiveTicketAsync(ticketId, _companyId.Value);
                        return NoContent();
                    }
                    if (User.IsInRole(nameof(Roles.ProjectManager)))
                    {
                        TicketDTO? ticketDTO = await _ticketService.GetTicketByIdAsync(ticketId, _companyId.Value);
                        if (ticketDTO == null) return BadRequest();

                        UserDTO? projectManager = await _projectService.GetProjectManagerAsync(ticketDTO.ProjectId, _companyId.Value);
                        if (projectManager == null || projectManager.Id != _userId) return BadRequest();

                        await _ticketService.ArchiveTicketAsync(ticketId, _companyId.Value);
                        return NoContent();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem();
            }
        }

        [HttpPut("{ticketId:int}/restore")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}")]
        public async Task<ActionResult> RestoreTicket([FromRoute] int ticketId)
        {
            try
            {
                if (_companyId != null)
                {
                    if (User.IsInRole(nameof(Roles.Admin)))
                    {
                        await _ticketService.ArchiveTicketAsync(ticketId, _companyId.Value);
                        return NoContent();
                    }
                    if (User.IsInRole(nameof(Roles.ProjectManager)))
                    {
                        TicketDTO? ticketDTO = await _ticketService.GetTicketByIdAsync(ticketId, _companyId.Value);
                        if (ticketDTO == null) return BadRequest();

                        UserDTO? projectManager = await _projectService.GetProjectManagerAsync(ticketDTO.ProjectId, _companyId.Value);
                        if (projectManager == null || projectManager.Id != _userId) return BadRequest();

                        await _ticketService.RestoreTicketAsync(ticketId, _companyId.Value);
                        return NoContent();
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem();
            }
        }

        [HttpPut("{ticketId:int}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}, {nameof(Roles.Developer)}, {nameof(Roles.Submitter)}")]
        public async Task<IActionResult> UpdateTicket(int ticketId, [FromBody] TicketDTO updatedTicket)
        {
            if (_companyId == null)
            {
                return BadRequest("Company ID not found.");
            }

            try
            {
                TicketDTO? existingTicket = await _ticketService.GetTicketByIdAsync(ticketId, _companyId.Value);
                if (existingTicket == null)
                {
                    return NotFound("Ticket not found.");
                }

                // Check authorization
                if (User.IsInRole(nameof(Roles.Admin)))
                {
                    // Admins can edit any ticket
                }
                else if (User.IsInRole(nameof(Roles.ProjectManager)))
                {
                    UserDTO? projectManager = await _projectService.GetProjectManagerAsync(existingTicket.ProjectId, _companyId.Value);
                    if (projectManager == null || projectManager.Id != _userId)
                    {
                        return BadRequest();
                    }
                }
                else if (User.IsInRole(nameof(Roles.Developer)))
                {
                    if (existingTicket.SubmitterUserId != _userId && existingTicket.DeveloperUserId != _userId)
                    {
                        return BadRequest();
                    }
                }
                else if (User.IsInRole(nameof(Roles.Submitter)))
                {
                    if (existingTicket.SubmitterUserId != _userId)
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest();
                }

                // Ensure that SubmitterUserId and ProjectId are not updated
                updatedTicket.SubmitterUserId = existingTicket.SubmitterUserId;
                updatedTicket.ProjectId = existingTicket.ProjectId;

                await _ticketService.UpdateTicketAsync(updatedTicket, _companyId.Value, _userId);
                return Ok(updatedTicket);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while updating the ticket.");
            }
        }

        [HttpGet("comments/{commentId:int}")]
        public async Task<ActionResult<TicketCommentDTO?>> GetCommentById([FromRoute] int commentId)
        {

            try
            {
                if (_companyId != null)
                {
                    TicketCommentDTO? comment = await _ticketService.GetCommentByIdAsync(commentId, _companyId.Value);
                    if (comment == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        return Ok(comment);
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem();
            }
        }

        [HttpGet("{ticketId:int}/comments")]
        public async Task<ActionResult<IEnumerable<TicketCommentDTO>>> GetTicketComments([FromRoute] int ticketId)
        {

            try
            {
                if (_companyId != null)
                {
                    IEnumerable<TicketCommentDTO> tickets = await _ticketService.GetTicketCommentsAsync(ticketId, _companyId.Value);
                    return Ok(tickets);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem();
            }

        }

        [HttpPost("comments")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}, {nameof(Roles.Developer)}, {nameof(Roles.Submitter)}")]
        public async Task<IActionResult> AddComment([FromBody] TicketCommentDTO comment)
        {
            if (_companyId == null)
            {
                return BadRequest("Company ID not found.");
            }

            try
            {
                TicketDTO? ticket = await _ticketService.GetTicketByIdAsync(comment.TicketId, _companyId.Value);
                if (ticket == null)
                {
                    return NotFound("Ticket not found.");
                }

                if (User.IsInRole(nameof(Roles.ProjectManager)))
                {
                    UserDTO? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId, _companyId.Value);
                    if (projectManager == null || projectManager.Id != _userId)
                    {
                        return Forbid();
                    }
                }
                else if (User.IsInRole(nameof(Roles.Developer)))
                {
                    if (ticket.DeveloperUserId != _userId || ticket.SubmitterUserId != _userId)
                    {
                        return Forbid();
                    }
                }
                else if (User.IsInRole(nameof(Roles.Submitter)))
                {
                    if (ticket.SubmitterUserId != _userId)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return Forbid();
                }

                await _ticketService.AddCommentAsync(comment, _companyId.Value);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while adding the comment.");
            }
        }


        [HttpPut("comments/{commentId:int}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}, {nameof(Roles.Developer)}, {nameof(Roles.Submitter)}")]
        public async Task<IActionResult> UpdateComment([FromBody] TicketCommentDTO comment)
        {
            try
            {
                if (_companyId != null)
                {
                    TicketDTO? ticket = await _ticketService.GetTicketByIdAsync(comment.TicketId, _companyId.Value);
                    if (User.IsInRole(nameof(Roles.ProjectManager)))
                    {
                        UserDTO? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId, _companyId.Value);
                        if (projectManager == null || projectManager.Id != _userId)
                        {
                            return Forbid();
                        }
                    }
                    else if (User.IsInRole(nameof(Roles.Developer)))
                    {
                        if(ticket.DeveloperUserId != _userId) return BadRequest();

                    }
                    else if (User.IsInRole(nameof(Roles.Submitter)))
                    {
                        if(ticket.SubmitterUserId != _userId) return BadRequest();
                    }
                    else
                    {
                        return BadRequest();
                    }
                    string userId = _userManager.GetUserId(User)!;
                    await _ticketService.UpdateCommentAsync(comment, userId);
                    return NoContent();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while updating the ticket.");
            }
        }

        [HttpDelete("comments/{commentId:int}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}, {nameof(Roles.Developer)}, {nameof(Roles.Submitter)}")]
        public async Task<IActionResult> DeleteComment([FromRoute] int commentId)
        {
            try
            {
                if (_companyId != null)
                {
                    TicketCommentDTO? comment = await _ticketService.GetCommentByIdAsync(commentId, _companyId.Value);
                    if (comment == null) return BadRequest();
                    TicketDTO? ticket = await _ticketService.GetTicketByIdAsync(comment.TicketId, _companyId.Value);
                    if (User.IsInRole(nameof(Roles.ProjectManager)))
                    {
                        UserDTO? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId, _companyId.Value);
                        if (projectManager == null || projectManager.Id != _userId)
                        {
                            return Forbid();
                        }
                    }
                    else if (User.IsInRole(nameof(Roles.Developer)))
                    {
                        if (ticket?.DeveloperUserId != _userId) return BadRequest();

                    }
                    else if (User.IsInRole(nameof(Roles.Submitter)))
                    {
                        if (ticket?.SubmitterUserId != _userId) return BadRequest();
                    }
                    else
                    {
                        return BadRequest();
                    }
                    string userId = _userManager.GetUserId(User)!;
                    await _ticketService.DeleteCommentAsync(commentId, _companyId.Value);
                    return NoContent();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while updating the ticket.");
            }
        }

        // DELETE: api/Tickets/attachments/1
        [HttpDelete("attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteTicketAttachment(int attachmentId)
        {

            if(_companyId is null) return BadRequest();

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            TicketAttachmentDTO attachment = await _ticketService.GetAttachmentById(attachmentId, _companyId.Value);
            if (user?.CompanyId == _companyId && User.IsInRole("Admin") || user?.Id == attachment.UserId)
            {

            await _ticketService.DeleteTicketAttachment(attachmentId, user.CompanyId);
            return NoContent();

            }
            else 
            {
                return BadRequest();
            }
        }

        // POST: api/Tickets/5/attachments
        // NOTE: the parameters are decorated with [FromForm] because they will be sent
        // encoded as multipart/form-data and NOT the typical JSON
        [HttpPost("{id}/attachments")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}, {nameof(Roles.Developer)}, {nameof(Roles.Submitter)}")]
        public async Task<ActionResult<TicketAttachmentDTO>> PostTicketAttachment(int id, [FromForm] TicketAttachmentDTO attachment, [FromForm] IFormFile? file)
        {
            if (attachment.TicketId != id || file is null)
            {
                return BadRequest("Invalid attachment data or file.");
            }

            ApplicationUser? user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            TicketDTO? ticket = await _ticketService.GetTicketByIdAsync(id, user.CompanyId);
            if (ticket == null)
            {
                return NotFound("Ticket not found.");
            }

            if (User.IsInRole(nameof(Roles.ProjectManager)))
            {
                UserDTO? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId, user.CompanyId);
                if (projectManager == null || projectManager.Id != user.Id)
                {
                    return BadRequest();
                }
            }
            else if (User.IsInRole(nameof(Roles.Developer)))
            {
                if (ticket.DeveloperUserId != user.Id && ticket.SubmitterUserId != user.Id)
                {
                    return BadRequest();
                }
            }
            else if (User.IsInRole(nameof(Roles.Submitter)))
            {
                if (ticket.SubmitterUserId != user.Id)
                {
                    return BadRequest();
                }
            }
            else
            {
                return BadRequest();
            }

            // Process the attachment
            attachment.UserId = user.Id;
            attachment.Created = DateTimeOffset.Now;

            if (string.IsNullOrWhiteSpace(attachment.FileName))
            {
                attachment.FileName = file.FileName;
            }

            // ImageHelper was renamed to UploadHelper!
            FileUpload upload = await UploadHelper.GetFileUploadAsync(file);

            try
            {
                var newAttachment = await _ticketService.AddTicketAttachment(attachment, upload.Data!, upload.Type!, user.CompanyId);
                return Ok(newAttachment);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while adding the attachment.");
            }
        }



    }
}
