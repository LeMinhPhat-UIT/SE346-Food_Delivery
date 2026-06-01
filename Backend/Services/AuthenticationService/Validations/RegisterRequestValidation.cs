using AuthenticationService.DTOs;
using FluentValidation;

namespace AuthenticationService.Validations
{
    public class CustomerRegistrationRequestValidation : AbstractValidator<CustomerRegistrationRequest>
    {
        public CustomerRegistrationRequestValidation()
        {
            RuleFor(request => request.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address");

            RuleFor(request => request.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");

            RuleFor(request => request.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .MinimumLength(9).WithMessage("Phone number must be at least 9 characters");
        }
    }

    public class VerifyOtpRequestValidation : AbstractValidator<VerifyOtpRequest>
    {
        public VerifyOtpRequestValidation()
        {
            RuleFor(request => request.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address");

            RuleFor(request => request.Otp)
                .NotEmpty().WithMessage("OTP is required")
                .Length(6).WithMessage("OTP must be 6 digits");
        }
    }

    public class SendOtpRequestValidation : AbstractValidator<SendOtpRequest>
    {
        public SendOtpRequestValidation()
        {
            RuleFor(request => request.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address");
        }
    }

    public class ChangePasswordRequestValidation : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidation()
        {
            RuleFor(request => request.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required");

            RuleFor(request => request.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters")
                .NotEqual(request => request.CurrentPassword).WithMessage("New password must be different from current password");

            RuleFor(request => request.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(request => request.NewPassword).WithMessage("Confirm password must match new password");
        }
    }
}
