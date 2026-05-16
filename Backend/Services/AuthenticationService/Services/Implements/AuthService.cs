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

        public async Task<ApiResponse<LoginResponse>> Login(LoginRequest request)
        {
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

                    return new ApiResponse<LoginResponse>(StatusCodes.Status403Forbidden, "User is locked");
                }
            }

            if (!result.Succeeded)
                return new ApiResponse<LoginResponse>(StatusCodes.Status401Unauthorized, "Invalid credentials");

            await _authRepository.ResetAccessFailedCountAsync(user);

            var deviceName = NormalizeDeviceName(request.DeviceName);
            var tokenGenerator = new JwtTokenGenerator(_jwtOptions, _authRepository);
            var accessToken = await tokenGenerator.AccessTokenGenerate(user);
            var refreshToken = await IssueRefreshTokenAsync(user.Id, deviceName);

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
            var refreshToken = await ValidateRefreshTokenAsync(request.RefreshToken, request.DeviceName);
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
            var refreshToken = await ValidateRefreshTokenAsync(request.RefreshToken, request.DeviceName);

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
            var newRefreshToken = await IssueRefreshTokenAsync(user.Id, refreshToken.DeviceName);

            return new ApiResponse<LoginResponse>(StatusCodes.Status200OK, new LoginResponse()
            {
                UserId = refreshToken.UserId,
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.Value.AccessTokenMinutes)
            });
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

        private async Task<string> IssueRefreshTokenAsync(Guid userId, string deviceName)
        {
            var refreshTokenValue = Guid.NewGuid().ToString("N");
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = refreshTokenValue,
                DeviceName = deviceName,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.Value.RefreshTokenMinutes),
                IsRevoked = false
            };

            await _authRepository.CreateRefreshTokenAsync(refreshToken);
            return refreshTokenValue;
        }

        private async Task<RefreshToken?> ValidateRefreshTokenAsync(string token, string deviceName)
        {
            var normalizedDeviceName = NormalizeDeviceName(deviceName);
            var refreshToken = await _authRepository.GetRefreshTokenAsync(token, normalizedDeviceName);

            if (
                refreshToken is null ||
                refreshToken.IsRevoked ||
                refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                return null;
            }

            return refreshToken;
        }

        private static string NormalizeDeviceName(string? deviceName)
        {
            return string.IsNullOrWhiteSpace(deviceName) ? "default" : deviceName.Trim();
        }
    }
}