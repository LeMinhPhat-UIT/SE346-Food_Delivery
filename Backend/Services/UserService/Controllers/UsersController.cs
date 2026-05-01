using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs.User;
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
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateUserProfile(Guid id, [FromBody] UpdateUserProfileRequest request)
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

        [HttpGet("{id:guid}/addresses")]
        [Authorize(Policy = "SelfOrAdmin")]
        public async Task<ActionResult<ApiResponse<PagedResult<UserAddressResponse>>>> GetUserAddresses(Guid id, [FromQuery] PaginationRequest paginationRequest)
        {
            try
            {
                var response = await _userService.GetAllUserAddressesByUserIdAsync(id, paginationRequest);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id:guid}/addresses/{addressId:guid}")]
        [Authorize(Policy = "SelfOrAdmin")]
        public async Task<ActionResult<ApiResponse<UserAddressResponse>>> GetUserAddress(Guid id, Guid addressId)
        {
            try
            {
                var addressResponse = await _userService.GetUserAddressByIdAsync(addressId);

                if (!addressResponse.Success)
                    return StatusCode(addressResponse.StatusCode, addressResponse);

                if (!IsCurrentUserAdmin() && addressResponse.Data.UserId != id)
                    return StatusCode(StatusCodes.Status403Forbidden, "You can only view your own addresses");

                return Ok(addressResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("{id:guid}/addresses")]
        [Authorize(Policy = "SelfOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> AddUserAddress(Guid id, [FromBody] CreateUserAddressRequest request)
        {
            try
            {
                var response = await _userService.AddUserAddressAsync(id, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id:guid}/addresses/{addressId:guid}")]
        [Authorize(Policy = "SelfOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> UpdateUserAddress(Guid id, Guid addressId, [FromBody] UpdateUserAddressRequest request)
        {
            try
            {
                var addressResponse = await _userService.GetUserAddressByIdAsync(addressId);

                if (!addressResponse.Success)
                    return StatusCode(addressResponse.StatusCode, addressResponse);

                if (!IsCurrentUserAdmin() && addressResponse.Data.UserId != id)
                    return StatusCode(StatusCodes.Status403Forbidden, "You can only update your own addresses");

                var response = await _userService.UpdateUserAddressAsync(addressId, request);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id:guid}/addresses/{addressId:guid}")]
        [Authorize(Policy = "SelfOrAdmin")]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> DeleteUserAddress(Guid id, Guid addressId)
        {
            try
            {
                var addressResponse = await _userService.GetUserAddressByIdAsync(addressId);

                if (!addressResponse.Success)
                    return StatusCode(addressResponse.StatusCode, addressResponse);

                if (!IsCurrentUserAdmin() && addressResponse.Data.UserId != id)
                    return StatusCode(StatusCodes.Status403Forbidden, "You can only delete your own addresses");

                var response = await _userService.DeleteUserAddressAsync(addressId);

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
