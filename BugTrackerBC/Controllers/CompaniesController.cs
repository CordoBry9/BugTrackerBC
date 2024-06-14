using BugTrackerBC.Client.Models;
using BugTrackerBC.Client.Services.Interfaces;
using BugTrackerBC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BugTrackerBC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {

        private readonly ICompanyDTOService _companyService;
        private readonly UserManager<ApplicationUser> _userManager;

        private int CompanyId => int.Parse(User.FindFirst("CompanyId")!.Value);

        private string UserId => _userManager.GetUserId(User)!;

        public CompaniesController(ICompanyDTOService companyService, UserManager<ApplicationUser> userManager)
        {
            _companyService = companyService;
            _userManager = userManager;
        }

        [HttpGet("{CompanyId}")]

        public async Task<ActionResult<CompanyDTO>> GetByIdCompany()
        {
            CompanyDTO? company = await _companyService.GetCompanyByIdAsync(CompanyId);

            if (company == null)
            {
                return NotFound();
            }

            return Ok(company);
        }

        [HttpGet("members")]

        public async Task<ActionResult<IEnumerable<UserDTO>>> GetCompanyMembers()
        {
            try
            {
                IEnumerable<UserDTO> members = await _companyService.GetCompanyMembersAsync(CompanyId);
                return Ok(members);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("members/{userId}/role")]

        public async Task<ActionResult<string?>> GetUserRole([FromRoute] string userId)
        {
            try
            {
                string? role = await _companyService.GetUserRoleAsync(userId, CompanyId);
                if (string.IsNullOrEmpty(role))
                {
                    return NotFound();
                }

                return Ok(role);    

            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        [HttpGet("{roleName}/users")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsersInRole([FromRoute] string roleName)
        {
            try
            {
                IEnumerable<UserDTO> users = await _companyService.GetUsersInRoleAsync(roleName, CompanyId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPut("update/{company.Id}")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        public async Task<IActionResult> UpdateCompany([FromBody] CompanyDTO company)
        {
            if (company == null || company.Id != CompanyId)
            {
                return BadRequest();
            }

            try
            {
                await _companyService.UpdateCompanyAsync(company, UserId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update/member")]
        [Authorize(Roles = $"{nameof(Roles.Admin)}")]
        public async Task<IActionResult> UpdateUserRoleAsync([FromBody] UserDTO user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            else
            {
                try
                {
                    await _companyService.UpdateUserRoleAsync(user, UserId);
                    return NoContent();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }


        }
    }
}
