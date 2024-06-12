using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using BugTrackerBC.Data;
using BugTrackerBC.Helpers.Extensions;
using BugTrackerBC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        private int? _companyId => User.FindFirst("CompanyId") != null ? int.Parse(User.FindFirst("CompanyId")!.Value) : null;

        public TicketsController(ITicketDTOService ticketService, UserManager<ApplicationUser> userManager)
        {
            _ticketService = ticketService;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<TicketDTO>> AddTicket([FromBody] TicketDTO newTicket)
        {
            if (_companyId != null)
            {
                TicketDTO ticket = await _ticketService.AddTicketAsync(newTicket, _companyId.Value);

                return Ok(ticket);
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

        [HttpPut("{ticketId:int}/archive")]
        public async Task<ActionResult> ArchiveTicket([FromRoute] int ticketId)
        {
            try
            {
                if (_companyId != null)
                {
                    await _ticketService.ArchiveTicketAsync(ticketId, _companyId.Value);
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

        [HttpPut("{ticketId:int}/restore")]
        public async Task<ActionResult> RestoreTicket([FromRoute] int ticketId)
        {
            try
            {
                if (_companyId != null)
                {
                    await _ticketService.RestoreTicketAsync(ticketId, _companyId.Value);
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
                return Problem();
            }
        }

        [HttpPut("{ticketId:int}")]
        public async Task<IActionResult> UpdateTicket([FromRoute] int ticketId, [FromBody] TicketDTO ticketDTO)
        {
            try
            {
                if (_companyId != null)
                {
                    string userId = _userManager.GetUserId(User)!;
                    await _ticketService.UpdateTicketAsync(ticketDTO, _companyId.Value, userId);
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

        public async Task<IActionResult> AddComment([FromBody] TicketCommentDTO comment)
        {
            if (_companyId != null)
            {
                await _ticketService.AddCommentAsync(comment, _companyId.Value);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("comments/{commentId:int}")]

        public async Task<IActionResult> UpdateComment([FromBody] TicketCommentDTO comment)
        {
            try
            {
                if (_companyId != null)
                {
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
        public async Task<IActionResult> DeleteComment([FromRoute] int commentId)
        {
            try
            {
                if (_companyId != null)
                {
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
                return Problem("An error occurred while deleting the ticket.");
            }
        }

        // POST: api/Tickets/5/attachments
        // NOTE: the parameters are decorated with [FromForm] because they will be sent
        // encoded as multipart/form-data and NOT the typical JSON
        [HttpPost("{id}/attachments")]
        public async Task<ActionResult<TicketAttachmentDTO>> PostTicketAttachment(int id,
                                                                                    [FromForm] TicketAttachmentDTO attachment,
                                                                                    [FromForm] IFormFile? file)
        {
            if (attachment.TicketId != id || file is null)
            {
                return BadRequest();
            }

            var user = await _userManager.GetUserAsync(User);
            var ticket = await _ticketService.GetTicketByIdAsync(id, user!.CompanyId);

            if (ticket is null)
            {
                return NotFound();
            }

            attachment.UserId = user!.Id;
            attachment.Created = DateTimeOffset.Now;

            if (string.IsNullOrWhiteSpace(attachment.FileName))
            {
                attachment.FileName = file.FileName;
            }

            // ImageHelper was renamed to UploadHelper!
            FileUpload upload = await UploadHelper.GetFileUploadAsync(file);

            try
            {
                var newAttachment = await _ticketService.AddTicketAttachment(attachment, upload.Data!, upload.Type!, user!.CompanyId);
                return Ok(newAttachment);
            }
            catch
            {
                return Problem();
            }
        }

        // DELETE: api/Tickets/attachments/1
        [HttpDelete("attachments/{attachmentId}")]
        public async Task<IActionResult> DeleteTicketAttachment(int attachmentId)
        {
            var user = await _userManager.GetUserAsync(User);

            await _ticketService.DeleteTicketAttachment(attachmentId, user!.CompanyId);

            return NoContent();
        }

    }
}
