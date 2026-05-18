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
}
