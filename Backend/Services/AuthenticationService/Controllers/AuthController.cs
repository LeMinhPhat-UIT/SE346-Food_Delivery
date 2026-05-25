using AuthenticationService.DTOs;
using AuthenticationService.Services.Interfaces;
using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthenticationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<RegisterResponse>>> Register([FromBody] CustomerRegistrationRequest request)
        {
            try
            {
                var response = await _authService.RegisterCustomerAsync(request);

                if (!response.Success)
                {
                    return StatusCode(response.StatusCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

        // lỗi verify: expired
        [HttpPost("verify-otp")]
        public async Task<ActionResult<ApiResponse<VerifyOtpResponse>>> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            try
            {
                var response = await _authService.VerifyOtpAsync(request);

                if (!response.Success)
                {
                    return StatusCode(response.StatusCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("resend-otp")]
        public async Task<ActionResult<ApiResponse<SendOtpResponse>>> ResendOtp([FromBody] SendOtpRequest request)
        {
            try
            {
                var response = await _authService.ResendOtpAsync(request.Email);

                if (!response.Success)
                {
                    return StatusCode(response.StatusCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.Login(request);

                if (!response.Success)
                {
                    return StatusCode(response.StatusCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var response = await _authService.RefreshToken(request);

                if (!response.Success)
                {
                    return StatusCode(response.StatusCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<LogoutResponse>>> Logout([FromBody] LogoutRequest request)
        {
            try
            {
                var response = await _authService.Logout(request);

                if (!response.Success)
                {
                    return StatusCode(response.StatusCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<ConfirmationResponse>>> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (!userId.HasValue)
                {
                    var invalidContextResponse = new ApiResponse<ConfirmationResponse>(
                        StatusCodes.Status401Unauthorized,
                        "Invalid user context");

                    return StatusCode(invalidContextResponse.StatusCode, invalidContextResponse);
                }

                var response = await _authService.ChangePasswordAsync(userId.Value, request);

                if (!response.Success)
                {
                    return StatusCode(response.StatusCode, response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("userId");

            return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        //[HttpPut("update-phone")]
        //public async Task<ActionResult<ApiResponse<UpdatePhoneNumberResponse> UpdatePhoneNumber([FromQuery] Guid userId, [FromBody] UpdatePhoneNumberRequest request)
        //{
        //    try
        //    {
        //        var result = await _authService.UpdatePhoneNumberAsync(userId, request.PhoneNumber);

        //        if (!result.Success)
        //        {
        //            return BadRequest(new
        //            {
        //                Success = false,
        //                Message = result.Message,
        //                Errors = result.Errors
        //            });
        //        }

        //        return Ok(new
        //        {
        //            Success = true,
        //            Message = result.Message
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        //    }
        //}
    }
}
