﻿using BugTrackerBC.Client.Models;
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

    }
}