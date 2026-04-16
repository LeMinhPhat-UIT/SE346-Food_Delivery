using AuthenticationService.DTOs;
using FluentValidation;

namespace AuthenticationService.Validations
{
    public class CustomerRegistrationRequestValidation : AbstractValidator<CustomerRegistrationRequest>
    {
        private readonly string PhoneNumberPattern = @"^(?:\+84|84|0)(3|5|7|8|9)\d{8}$";

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
        private readonly string PhoneNumberPattern = @"^(?:\+84|84|0)(3|5|7|8|9)\d{8}$";

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
        private readonly string PhoneNumberPattern = @"^(?:\+84|84|0)(3|5|7|8|9)\d{8}$";

        public SendOtpRequestValidation()
        {
            RuleFor(request => request.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email address");
        }
    }
}
