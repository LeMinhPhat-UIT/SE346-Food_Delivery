using AuthenticationService.DTOs;
using Messaging.Contracts.Common;

namespace AuthenticationService.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<RegisterResponse>> RegisterCustomerAsync(CustomerRegistrationRequest request);
        Task<ApiResponse<VerifyOtpResponse>> VerifyOtpAsync(VerifyOtpRequest request);
        Task<ApiResponse<SendOtpResponse>> ResendOtpAsync(string email);
        Task<ApiResponse<SendOtpResponse>> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<ApiResponse<VerifyResetOtpResponse>> VerifyResetOtpAsync(VerifyResetOtpRequest request);
        Task<ApiResponse<ConfirmationResponse>> ResetPasswordAsync(ResetPasswordRequest request);
        Task<ApiResponse<LoginResponse>> Login(LoginRequest request);
        Task<ApiResponse<LogoutResponse>> Logout(LogoutRequest request);
        Task<ApiResponse<LoginResponse>> RefreshToken(RefreshTokenRequest request);
        Task<ApiResponse<ConfirmationResponse>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
        //Task<AuthResult> UpdatePhoneNumberAsync(UpdatePhoneNumberDTO dto);
    }
}
