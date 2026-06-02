using AuthenticationService.DTOs;
using AuthenticationService.Entities;
using AuthenticationService.Enums;
using AuthenticationService.Helpers;
using AuthenticationService.Mappers;
using AuthenticationService.Options;
using AuthenticationService.Repositories.Interfaces;
using AuthenticationService.Services.Interfaces;
using Messaging.Contracts.Common;
using Messaging.Contracts.Events;
using Messaging.RabbitMq.Publishing;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace AuthenticationService.Services.Implements
{
    public class AuthService : IAuthService
    {
        private const string DeviceIdHeaderName = "X-Device-Id";
        private const string DeviceNameHeaderName = "X-Device-Name";

        private readonly IAuthRepository _authRepository;
        private readonly CustomerRegisterRequestMapper _customerRegisterRequestMapper;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILogger<AuthService> _logger;
        private readonly IOptions<AuthenticationOptions> _options;
        private readonly IOptions<JwtOptions> _jwtOptions;

        private readonly int OTP_EXPIRY_SECONDS;

        public AuthService(
            IAuthRepository authRepository,
            CustomerRegisterRequestMapper customerRegisterRequestMapper,
            IEventPublisher eventPublisher,
            ILogger<AuthService> logger,
            IOptions<AuthenticationOptions> options,
            IOptions<JwtOptions> jwtOptions)
        {
            _authRepository = authRepository;
            _customerRegisterRequestMapper = customerRegisterRequestMapper;
            _eventPublisher = eventPublisher;
            _logger = logger;
            _options = options;
            _jwtOptions = jwtOptions;

            OTP_EXPIRY_SECONDS = _options.Value.OtpSettings.DefaultOtpExpiredTimeSpanInSeconds;
        }

        public async Task<ApiResponse<RegisterResponse>> RegisterCustomerAsync(CustomerRegistrationRequest request)
        {
            var existingUser = await _authRepository.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                if (existingUser.IsOtpVerified)
                {
                    return new ApiResponse<RegisterResponse>(400, new List<string>
                    {
                        "Email already registered"
                    });
                }

                var otp = GenerateOtp();
                existingUser.FullName = string.IsNullOrWhiteSpace(request.FullName) ? existingUser.FullName : request.FullName;
                existingUser.PhoneNumber = request.PhoneNumber;
                existingUser.Otp = otp;
                existingUser.OtpExpiresAt = DateTime.UtcNow.AddSeconds(OTP_EXPIRY_SECONDS);

                await _authRepository.UpdateUserAsync(existingUser);

                var otpEvent = new OtpSendRequestedEvent(existingUser.Id, existingUser.Email!, otp)
                {
                    ExpiresAt = existingUser.OtpExpiresAt.Value
                };

                await PublishOtpEventAsync(otpEvent);

                return new ApiResponse<RegisterResponse>(200, new RegisterResponse
                {
                    UserId = existingUser.Id,
                    Email = existingUser.Email!,
                    Message = "OTP resent. Please verify your email",
                    RequiresOtpVerification = true
                });
            }

            var user = _customerRegisterRequestMapper.ToApplicationUser(request);

            var otpCode = GenerateOtp();
            user.Otp = otpCode;
            user.OtpExpiresAt = DateTime.UtcNow.AddSeconds(OTP_EXPIRY_SECONDS);

            var result = await _authRepository.RegisterUserAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new ApiResponse<RegisterResponse>(400,
                    result.Errors.Select(e => e.Description).ToList());
            }

            await _authRepository.AddToRoleAsync(user, "Customer");

            var userCreatedEvent = new UserCreatedEvent
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty,
                CorrelationId = Guid.NewGuid().ToString()
            };

            await _eventPublisher.PublishAsync(userCreatedEvent);

            var otpSendRequestedEvent = new OtpSendRequestedEvent(user.Id, user.Email!, otpCode)
            {
                ExpiresAt = user.OtpExpiresAt.Value
            };

            await PublishOtpEventAsync(otpSendRequestedEvent);

            return new ApiResponse<RegisterResponse>(200, new RegisterResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                Message = "Registration successful. Please verify OTP",
                RequiresOtpVerification = true
            });
        }

        public async Task<ApiResponse<VerifyOtpResponse>> VerifyOtpAsync(VerifyOtpRequest request)
        {
            var user = await _authRepository.FindByEmailAsync(request.Email);
            if (user == null)
                return new ApiResponse<VerifyOtpResponse>(StatusCodes.Status404NotFound, "Email invalid");

            if (user.IsOtpVerified)
                return new ApiResponse<VerifyOtpResponse>(StatusCodes.Status409Conflict, "User is already verified");

            if (user.OtpExpiresAt < DateTime.UtcNow)
                return new ApiResponse<VerifyOtpResponse>(StatusCodes.Status410Gone, "OTP has expired");

            if (user.Otp != request.Otp)
                return new ApiResponse<VerifyOtpResponse>(StatusCodes.Status400BadRequest, "OTP invalid");

            user.IsOtpVerified = true;
            user.EmailConfirmed = true;
            user.Status = AuthStatus.Active;
            user.Otp = null;
            user.OtpExpiresAt = null;

            await _authRepository.UpdateUserAsync(user);

            var otpVerifiedEvent = new OtpVerifiedEvent
            {
                UserId = user.Id,
                CorrelationId = Guid.NewGuid().ToString()
            };

            await _eventPublisher.PublishAsync(otpVerifiedEvent);

            _logger.LogInformation("User {UserId} verified successfully", user.Id);

            return new ApiResponse<VerifyOtpResponse>(StatusCodes.Status200OK, new VerifyOtpResponse { Message = "Email verified successfully" });
        }

        public async Task<ApiResponse<SendOtpResponse>> ResendOtpAsync(string email)
        {
            var user = await _authRepository.FindByEmailAsync(email);
            if (user == null)
                return new ApiResponse<SendOtpResponse>(StatusCodes.Status404NotFound, "Email invalid");

            if (user.IsOtpVerified)
                return new ApiResponse<SendOtpResponse>(StatusCodes.Status409Conflict, "User is already verified");

            var otp = GenerateOtp();
            user.Otp = otp;
            user.OtpExpiresAt = DateTime.UtcNow.AddSeconds(OTP_EXPIRY_SECONDS);

            await _authRepository.UpdateUserAsync(user);

            //[note] maybe will apply design pattern in future
            var otpSendRequestedEvent = new OtpSendRequestedEvent(user.Id, user.Email!, otp)
            {
                ExpiresAt = user.OtpExpiresAt.Value
            };

            await PublishOtpEventAsync(otpSendRequestedEvent);

            return new ApiResponse<SendOtpResponse>(StatusCodes.Status200OK, new SendOtpResponse
            {
                Message = "OTP resent successfully",
                ExpiresInSeconds = OTP_EXPIRY_SECONDS
            });
        }

        public async Task<ApiResponse<SendOtpResponse>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _authRepository.FindByEmailAsync(request.Email);
            if (user == null)
                return new ApiResponse<SendOtpResponse>(StatusCodes.Status404NotFound, "Email invalid");

            if (!user.IsOtpVerified)
                return new ApiResponse<SendOtpResponse>(StatusCodes.Status409Conflict, "User email is not verified");

            var otp = GenerateOtp();
            user.Otp = otp;
            user.OtpExpiresAt = DateTime.UtcNow.AddSeconds(OTP_EXPIRY_SECONDS);

            await _authRepository.UpdateUserAsync(user);

            var otpSendRequestedEvent = new OtpSendRequestedEvent(user.Id, user.Email!, otp)
            {
                OtpType = "password-reset",
                ExpiresAt = user.OtpExpiresAt.Value
            };

            await PublishOtpEventAsync(otpSendRequestedEvent);

            return new ApiResponse<SendOtpResponse>(StatusCodes.Status200OK, new SendOtpResponse
            {
                Message = "Password reset OTP sent successfully",
                ExpiresInSeconds = OTP_EXPIRY_SECONDS
            });
        }

        public async Task<ApiResponse<VerifyResetOtpResponse>> VerifyResetOtpAsync(VerifyResetOtpRequest request)
        {
            var user = await _authRepository.FindByEmailAsync(request.Email);
            if (user == null)
                return new ApiResponse<VerifyResetOtpResponse>(StatusCodes.Status404NotFound, "Email invalid");

            if (!user.IsOtpVerified)
                return new ApiResponse<VerifyResetOtpResponse>(StatusCodes.Status409Conflict, "User email is not verified");

            if (!user.OtpExpiresAt.HasValue || user.OtpExpiresAt.Value < DateTime.UtcNow)
                return new ApiResponse<VerifyResetOtpResponse>(StatusCodes.Status410Gone, "OTP has expired");

            if (user.Otp != request.Otp)
                return new ApiResponse<VerifyResetOtpResponse>(StatusCodes.Status400BadRequest, "OTP invalid");

            var resetToken = await _authRepository.GeneratePasswordResetTokenAsync(user);

            user.Otp = null;
            user.OtpExpiresAt = null;
            await _authRepository.UpdateUserAsync(user);

            return new ApiResponse<VerifyResetOtpResponse>(StatusCodes.Status200OK, new VerifyResetOtpResponse
            {
                Message = "Reset OTP verified successfully",
                ResetToken = resetToken
            });
        }

        public async Task<ApiResponse<ConfirmationResponse>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "New password and confirmation password do not match");

            var user = await _authRepository.FindByEmailAsync(request.Email);
            if (user == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "Email invalid");

            var result = await _authRepository.ResetPasswordAsync(user, request.ResetToken, request.NewPassword);
            if (!result.Succeeded)
            {
                return new ApiResponse<ConfirmationResponse>(
                    StatusCodes.Status400BadRequest,
                    result.Errors.Select(error => error.Description).ToList());
            }

            user.Otp = null;
            user.OtpExpiresAt = null;
            await _authRepository.UpdateUserAsync(user);

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Reset password successfully"));
        }

        public async Task<ApiResponse<LoginResponse>> Login(LoginRequest request)
        {
            var deviceId = NormalizeRequiredHeaderValue(request.DeviceId);
            if (deviceId is null)
                return new ApiResponse<LoginResponse>(StatusCodes.Status400BadRequest, $"{DeviceIdHeaderName} header is required");

            var deviceName = NormalizeRequiredHeaderValue(request.DeviceName);
            if (deviceName is null)
                return new ApiResponse<LoginResponse>(StatusCodes.Status400BadRequest, $"{DeviceNameHeaderName} header is required");

            var user = await _authRepository.FindByEmailAsync(request.Email);
            if (user is null || !user.IsOtpVerified)
                return new ApiResponse<LoginResponse>(StatusCodes.Status404NotFound, "Invalid credentials");

            var wasLocked = await _authRepository.IsLockedOutAsync(user);

            var result = await _authRepository.CheckPasswordSignInAsync(
                user,
                request.Password,
                _options.Value.LockoutSettings.IsLockoutOnFailure
            );

            if (result.IsLockedOut)
            {
                if (!wasLocked)
                {
                    var lockedOutEvent = new LockedOutEvent()
                    {
                        UserId = user.Id,
                        Email = user.Email!,
                        Message = "Your account is locked due to failed access multilple time",
                        LockoutEndDate = DateTime.UtcNow.AddMinutes(_options.Value.LockoutSettings.DefaultLockoutTimeSpanInMinutes)
                    };

                    await _eventPublisher.PublishAsync(lockedOutEvent);
                }

                return new ApiResponse<LoginResponse>(StatusCodes.Status403Forbidden, "User is locked");
            }

            if (!result.Succeeded)
                return new ApiResponse<LoginResponse>(StatusCodes.Status401Unauthorized, "Invalid credentials");

            await _authRepository.ResetAccessFailedCountAsync(user);

            var tokenGenerator = new JwtTokenGenerator(_jwtOptions, _authRepository);
            var accessToken = await tokenGenerator.AccessTokenGenerate(user);
            var refreshToken = await IssueRefreshTokenAsync(user.Id, deviceId, deviceName);

            return new ApiResponse<LoginResponse>(StatusCodes.Status200OK, new LoginResponse
            {
                AccessToken = accessToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.Value.AccessTokenMinutes),
                UserId = user.Id,
                RefreshToken = refreshToken
            });
        }

        public async Task<ApiResponse<LogoutResponse>> Logout(LogoutRequest request)
        {
            var deviceId = NormalizeRequiredHeaderValue(request.DeviceId);
            if (deviceId is null)
                return new ApiResponse<LogoutResponse>(StatusCodes.Status400BadRequest, $"{DeviceIdHeaderName} header is required");

            var refreshToken = await ValidateRefreshTokenAsync(request.RefreshToken, deviceId);
            if (refreshToken is null)
            {
                return new ApiResponse<LogoutResponse>(StatusCodes.Status400BadRequest, "Invalid refresh token");
            }

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            await _authRepository.UpdateRefreshTokenAsync(refreshToken);

            return new ApiResponse<LogoutResponse>(StatusCodes.Status200OK, new LogoutResponse
            {
                Message = "Logout successfully"
            });
        }

        public async Task<ApiResponse<LoginResponse>> RefreshToken(RefreshTokenRequest request)
        {
            var deviceId = NormalizeRequiredHeaderValue(request.DeviceId);
            if (deviceId is null)
                return new ApiResponse<LoginResponse>(StatusCodes.Status400BadRequest, $"{DeviceIdHeaderName} header is required");

            var refreshToken = await ValidateRefreshTokenAsync(request.RefreshToken, deviceId);

            if (refreshToken is null)
            {
                return new ApiResponse<LoginResponse>(StatusCodes.Status400BadRequest, "Invalid refresh token");
            }

            var user = await _authRepository.FindByIdAsync(refreshToken.UserId);
            if (user is null)
            {
                return new ApiResponse<LoginResponse>(StatusCodes.Status404NotFound, "User not found");
            }

            refreshToken.IsRevoked = true;
            refreshToken.RevokedAt = DateTime.UtcNow;

            await _authRepository.UpdateRefreshTokenAsync(refreshToken);

            var tokenGenerator = new JwtTokenGenerator(_jwtOptions, _authRepository);
            var accessToken = await tokenGenerator.AccessTokenGenerate(user);
            var newRefreshToken = await IssueRefreshTokenAsync(user.Id, refreshToken.DeviceId, refreshToken.DeviceName);

            return new ApiResponse<LoginResponse>(StatusCodes.Status200OK, new LoginResponse()
            {
                UserId = refreshToken.UserId,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.Value.AccessTokenMinutes)
            });
        }

        public async Task<ApiResponse<ConfirmationResponse>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "New password and confirmation password do not match");

            if (request.CurrentPassword == request.NewPassword)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "New password must be different from current password");

            var user = await _authRepository.FindByIdAsync(userId);
            if (user is null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "User not found");

            var result = await _authRepository.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return new ApiResponse<ConfirmationResponse>(
                    StatusCodes.Status400BadRequest,
                    result.Errors.Select(error => error.Description).ToList());
            }

            return new ApiResponse<ConfirmationResponse>(
                StatusCodes.Status200OK,
                new ConfirmationResponse("Change password successfully"));
        }

        private static string GenerateOtp()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        private async Task PublishOtpEventAsync(OtpSendRequestedEvent @event)
        {
            await _eventPublisher.PublishAsync(@event);
            _logger.LogInformation("OTP event published for user {UserId}", @event.UserId);
        }

        private async Task<string> IssueRefreshTokenAsync(Guid userId, string deviceId, string deviceName)
        {
            var refreshTokenValue = Guid.NewGuid().ToString("N");
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = refreshTokenValue,
                DeviceId = deviceId,
                DeviceName = deviceName,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.Value.RefreshTokenMinutes),
                IsRevoked = false
            };

            await _authRepository.CreateRefreshTokenAsync(refreshToken);
            return refreshTokenValue;
        }

        private async Task<RefreshToken?> ValidateRefreshTokenAsync(string? token, string? deviceId)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            token = token.Trim();
            var normalizedDeviceId = NormalizeRequiredHeaderValue(deviceId);
            if (normalizedDeviceId is null)
                return null;

            var refreshToken = await _authRepository.GetRefreshTokenAsync(token, normalizedDeviceId);

            if (
                refreshToken is null ||
                refreshToken.IsRevoked ||
                refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return null;
            }

            return refreshToken;
        }

        private static string? NormalizeRequiredHeaderValue(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
