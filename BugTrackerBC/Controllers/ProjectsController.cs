using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using BugTrackerBC.Data;
using BugTrackerBC.Helpers.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Security.Claims;

namespace BugTrackerBC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : ControllerBase
    {

        private readonly IProjectDTOService _projectService; //private read only fields

        private readonly ICompanyDTOService _companyService;

        private readonly UserManager<ApplicationUser> _userManager;
        private int? _companyId => User.FindFirst("CompanyId") != null ? int.Parse(User.FindFirst("CompanyId")!.Value) : null;

        private string _userId => _userManager.GetUserId(User)!;

        public ProjectsController(IProjectDTOService projectService, UserManager<ApplicationUser> userManager, ICompanyDTOService companyService)
        {
            _projectService = projectService;
            _userManager = userManager;
            _companyService = companyService;
        }
        //[AllowAnonymous] can make it so just this method can be used by anyone if controller is fully authorized

        [HttpPost]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}")]
        public async Task<ActionResult<ProjectDTO>> AddProject([FromBody] ProjectDTO newComment)
        {

            ApplicationUser? user = await _userManager.GetUserAsync(User);

            if (user?.CompanyId != _companyId) return BadRequest();

            if (User.IsInRole(nameof(Roles.ProjectManager)) || User.IsInRole(nameof(Roles.Admin)))
            {

                if (_companyId != null)
                {
                    ProjectDTO comment = await _projectService.AddProjectAsync(newComment, _companyId.Value, _userId);

                    return Ok(comment);
                }
                else
                {
                    return BadRequest("CompanyId not found in claims.");
                }
            }
            else
            {
                return BadRequest();
            }

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetAllProjects()
        {

            ApplicationUser? user = await _userManager.GetUserAsync(User);

            if (user?.CompanyId != _companyId) return BadRequest();

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

        [HttpPut("{projectId:int}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}")]
        public async Task<ActionResult> UpdateProject([FromRoute] int projectId, [FromBody] ProjectDTO projectDTO)
        {
            if (projectId != projectDTO.Id) return BadRequest("Project ID mismatch.");

            try
            {
                // Check if the user is an Admin
                if (User.IsInRole(nameof(Roles.Admin)))
                {
                    if (_companyId != null)
                    {
                        await _projectService.UpdateProjectAsync(projectDTO, _companyId.Value);
                        return Ok();
                    }
                    else
                    {
                        return BadRequest("company Id is null");
                    }
                }

                // Check if the user is a Project Manager
                if (User.IsInRole(nameof(Roles.ProjectManager)))
                {
                    // Get the project manager of the project
                    UserDTO? projectManager = await _projectService.GetProjectManagerAsync(projectId, _companyId!.Value);
                    if (projectManager == null) return BadRequest("Project manager is null");


                    // Verify if the current user is the project manager
                    if (_userId == projectManager.Id)
                    {
                        if (_companyId != null)
                        {
                            await _projectService.UpdateProjectAsync(projectDTO, _companyId.Value);
                            return Ok();
                        }
                        else
                        {
                            return BadRequest("company Id is null");
                        }
                    }
                    else
                    {
                        return BadRequest("logged in user does not match project manager id");
                    }
                }

                // If the user is neither Admin nor Project Manager
                return Forbid("You are not authorized to update this project.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while updating the project.");
            }
        }

        [HttpPut("{projectId:int}/manager")]
        [Authorize(Roles = "Admin")] //redundancy, but just testing various auth methods
        public async Task<IActionResult> AssignProjectManager([FromRoute] int projectId, [FromBody] string memberId)
        {
            try
            {
                //check if user is Admin role
                if (User.IsInRole(nameof(Roles.Admin)) && _companyId != null)
                {
                    ProjectDTO? theirProject = await _projectService.GetProjectByIdAsync(projectId, _companyId.Value);
                    //uses _companyId of the logged in user to check if the projectId sent is part of the admins company
                    if (theirProject == null) return NotFound();

                    await _projectService.AssignProjectManagerAsync(projectId, memberId, _userId);
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
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}")]
        public async Task<IActionResult> AddMemberToProject([FromRoute] int projectId, [FromBody] string memberId)
        {
            try
            {

                if (User.IsInRole(nameof(Roles.Admin)) && _companyId != null)
                {
                    await _projectService.AddMemberToProjectAsync(projectId, memberId, _userId);
                    return NoContent();
                }
                else if (User.IsInRole(nameof(Roles.ProjectManager)) && _companyId != null)
                {
                    UserDTO? projectManager = await _projectService.GetProjectManagerAsync(projectId, _companyId!.Value);
                    if (projectManager != null)
                    {
                        await _projectService.AddMemberToProjectAsync(projectId, memberId, projectManager.Id!);
                        return NoContent();
                    }
                    else
                    {
                        return BadRequest();
                    }

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

        [HttpGet("member/{memberId}/activeprojects")]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetMemberProjects([FromRoute] string memberId)
        {


            IEnumerable<ProjectDTO> projectMembers;

            if (_companyId != null)
            {

                projectMembers = await _projectService.GetMemberProjectsAsync(_companyId.Value, memberId);
                return Ok(projectMembers);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("member/{memberId}/archivedprojects")]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetMemberArchivedProjects([FromRoute] string memberId)
        {

            IEnumerable<ProjectDTO> projectMembers;

            if (_companyId != null)
            {

                projectMembers = await _projectService.GetMemberArchivedProjectsAsync(_companyId.Value, memberId);
                return Ok(projectMembers);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut("{projectId:int}/archive")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}")]
        public async Task<ActionResult<ProjectDTO>> ArchiveProject([FromRoute] int projectId)
        {
            try
            {
                if (User.IsInRole(nameof(Roles.Admin)))
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

                if (User.IsInRole(nameof(Roles.ProjectManager)))
                {
                    UserDTO? projectManager = await _projectService.GetProjectManagerAsync(projectId, _companyId!.Value);
                    if (projectManager == null) return BadRequest("There is not project manager assigned to this project");

                    if (_userId == projectManager.Id)
                    {
                        if (_companyId != null)
                        {
                            await _projectService.ArchiveProjectAsync(projectId, _companyId.Value);
                            return Ok();
                        }
                        else
                        {
                            return BadRequest("company Id is null");
                        }
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                return Forbid("You are not authorized to update this project.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while fetching projects.");
            }

        }

        [HttpPut("{projectId:int}/restore")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}, {nameof(Roles.ProjectManager)}")]
        public async Task<ActionResult> RestoreProject([FromRoute] int projectId)
        {
            try
            {
                if (User.IsInRole(nameof(Roles.Admin)))
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

                if (User.IsInRole(nameof(Roles.ProjectManager)))
                {
                    UserDTO? projectManager = await _projectService.GetProjectManagerAsync(projectId, _companyId!.Value);
                    if (projectManager == null) return BadRequest("There is not project manager assigned to this project");

                    if (_userId == projectManager.Id)
                    {
                        if (_companyId != null)
                        {
                            await _projectService.RestoreProjectAsync(projectId, _companyId.Value);
                            return Ok();
                        }
                        else
                        {
                            return BadRequest("company Id is null");
                        }
                    }
                    else
                    {
                        return BadRequest();
                    }

                }

                return Forbid("You are not authorized to update this project.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Problem("An error occurred while restoring the project.");
            }



        }
    }
}
