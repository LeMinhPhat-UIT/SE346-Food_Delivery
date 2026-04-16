using AuthenticationService.DTOs;
using Messaging.Contracts.Common;

namespace AuthenticationService.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<RegisterResponse>> RegisterCustomerAsync(CustomerRegistrationRequest request);
        Task<ApiResponse<VerifyOtpResponse>> VerifyOtpAsync(VerifyOtpRequest request);
        Task<ApiResponse<SendOtpResponse>> ResendOtpAsync(string email);
        Task<ApiResponse<LoginResponse>> Login(LoginRequest request);
        Task<ApiResponse<LogoutResponse>> RevokeToken(LogoutRequest request);
        //Task<AuthResult> UpdatePhoneNumberAsync(UpdatePhoneNumberDTO dto);
    }
}
