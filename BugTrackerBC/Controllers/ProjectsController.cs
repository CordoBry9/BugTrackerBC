using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using BugTrackerBC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

namespace BugTrackerBC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {

        private readonly IProjectDTOService _projectService; //private read only fields

        private readonly UserManager<ApplicationUser> _userManager;
        private int? _companyId => User.FindFirst("CompanyId") != null ? int.Parse(User.FindFirst("CompanyId")!.Value) : null;

        public ProjectsController(IProjectDTOService projectService, UserManager<ApplicationUser> userManager)
        {
            _projectService = projectService;
            _userManager = userManager;
        }
        //[AllowAnonymous] can make it so just this method can be used by anyone if controller is fully authorized

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ProjectDTO>> AddProject([FromBody] ProjectDTO newComment)
        {

            if (_companyId != null)
            {
                ProjectDTO comment = await _projectService.AddProjectAsync(newComment, _companyId.Value);

                return Ok(comment);
            }
            else
            {
                return BadRequest("CompanyId not found in claims.");
            }

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetAllProjects()
        {
            try
            {
                if (_companyId != null)
                {
                    IEnumerable<ProjectDTO> projects = await _projectService.GetAllProjectsAsync(_companyId.Value);
                    return Ok(projects);
                }
                return BadRequest("CompanyId not found in claims.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while fetching projects.");
            }
        }

        [HttpPut("{projectId:int}/archive")]
        public async Task<ActionResult<ProjectDTO>> ArchiveProject([FromRoute] int projectId)
        {
            try
            {
                if (_companyId != null)
                {
                    await _projectService.ArchiveProjectAsync(projectId, _companyId.Value);
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
                return Problem("An error occurred while fetching projects.");
            }

        }

        [HttpGet("{projectId}")]
        public async Task<ActionResult<ProjectDTO?>> GetProjectById([FromRoute] int projectId)
        {
            try
            {
                if (_companyId != null)
                {
                    ProjectDTO? project = await _projectService.GetProjectByIdAsync(projectId, _companyId.Value);
                    if (project == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        return Ok(project);
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

        [HttpGet("archived")]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetArchivedProjects()
        {
            if (_companyId != null)
            {
                IEnumerable<ProjectDTO> projects = await _projectService.GetArchivedProjects(_companyId.Value);
                return Ok(projects);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("{projectId:int}/restore")]
        public async Task<ActionResult> RestoreProject([FromRoute] int projectId)
        {
            try
            {
                if (_companyId != null)
                {
                    await _projectService.RestoreProjectAsync(projectId, _companyId.Value);
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
                return Problem("An error occurred while restoring the project.");
            }
        }

        [HttpPut("{projectId:int}")]
        public async Task<ActionResult> UpdateProject([FromRoute] int projectId, [FromBody] ProjectDTO projectDTO)
        {
            try
            {
                if (_companyId != null)
                {
                    await _projectService.UpdateProjectAsync(projectDTO, _companyId.Value);
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
                return Problem("An error occurred while updating the project.");
            }
        }


        [HttpPut("{projectId:int}/manager")]
        public async Task<IActionResult> AssignProjectManager([FromRoute] int projectId, [FromBody] string memberId)
        {
            try
            {
                ApplicationUser? admin = await _userManager.GetUserAsync(User);
                if (admin != null && _companyId != null)
                {
                    await _projectService.AssignProjectManagerAsync(projectId, memberId, admin.Id);
                    return NoContent();
                }
                else
                {
                    return BadRequest("Unable to identify admin or company ID.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while assigning the project manager.");
            }
        }

        [HttpDelete("{projectId:int}/members/{memberId}")]
        public async Task<IActionResult> RemoveMemberFromProject([FromRoute] int projectId, [FromRoute] string memberId)
        {
            try
            {
                var manager = await _userManager.GetUserAsync(User);
                if (manager != null && _companyId != null)
                {
                    await _projectService.RemoveMemberFromProjectAsync(projectId, memberId, manager.Id);
                    return NoContent();
                }
                else
                {
                    return BadRequest("Unable to identify manager or company ID.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while removing the member from the project.");
            }
        }

        [HttpDelete("{projectId:int}/manager")]
        public async Task<IActionResult> RemoveProjectManager([FromRoute] int projectId)
        {
            try
            {
                var admin = await _userManager.GetUserAsync(User);
                if (admin != null && _companyId != null)
                {
                    await _projectService.RemoveProjectManagerAsync(projectId, admin.Id);
                    return NoContent();
                }
                else
                {
                    return BadRequest("Unable to identify admin or company ID.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while removing the project manager.");
            }
        }

        [HttpPut("{projectId:int}/addmembers")]
        public async Task<IActionResult> AddMemberToProject([FromRoute] int projectId, [FromBody] string memberId)
        {
            try
            {
                ApplicationUser? manager = await _userManager.GetUserAsync(User);
                if (manager != null && _companyId != null)
                {
                    await _projectService.AddMemberToProjectAsync(projectId, memberId, manager.Id);
                    return NoContent();
                }
                else
                {
                    return BadRequest("Unable to identify manager or company ID.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while adding the member to the project.");
            }
        }


        [HttpGet("{projectId:int}/manager")]
        public async Task<ActionResult<UserDTO?>> GetProjectManager([FromRoute] int projectId)
        {
            try
            {
                if (_companyId != null)
                {
                    UserDTO? projectManager = await _projectService.GetProjectManagerAsync(projectId, _companyId.Value);
                    if (projectManager == null)
                    {
                        return NoContent(); // Return 204 if no manager is assigned
                    }
                    return Ok(projectManager);
                }
                else
                {
                    return BadRequest("CompanyId not found in claims.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while fetching the project manager.");
            }
        }



        [HttpGet("{projectId:int}/members")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetProjectMembers([FromRoute] int projectId)
        {
            IEnumerable<UserDTO> projectMembers;

            if (_companyId != null)
            {
                projectMembers = await _projectService.GetProjectMembersAsync(projectId, _companyId.Value);
                return Ok(projectMembers);
            }
            else
            {
                return BadRequest();
            }

        }
    }
}
