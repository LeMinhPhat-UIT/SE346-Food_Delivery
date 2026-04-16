using AuthenticationService.DTOs;
using AuthenticationService.Services.Interfaces;
using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<LogoutResponse>>> Logout([FromBody] LogoutRequest request)
        {
            try
            {

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
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
