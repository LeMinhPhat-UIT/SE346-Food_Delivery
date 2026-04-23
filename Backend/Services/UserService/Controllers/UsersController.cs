using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Entities;
using UserService.Enums;
using UserService.Services.Interfaces;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<ApiResponse<PagedResult<UserProfileResponse>>>> GetAllUsers([FromQuery] PaginationRequest pagedOption)
        {
            try
            {
                var response = await _userService.GetAllUserAsync(pagedOption);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "SelfOrAdmin")]
        public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetUserProfile(Guid id)
        {
            try
            {
                var response = await _userService.GetUserAsync(id);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "SelfOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateUserProfile(Guid id, [FromBody] UserProfileUpdateRequest request)
        {
            try
            {
                var response = await _userService.UpdateUserProfileAsync(id, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "SelfOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> DeleteUser(Guid id)
        {
            try
            {
                var response = await _userService.DeleteUserAsync(id);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
