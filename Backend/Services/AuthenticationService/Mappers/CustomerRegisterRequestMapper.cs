using AuthenticationService.DTOs;
using AuthenticationService.Entities;
using Riok.Mapperly.Abstractions;

namespace AuthenticationService.Mappers
{
    [Mapper]
    public partial class CustomerRegisterRequestMapper
    {
        public ApplicationUser ToApplicationUser(CustomerRegistrationRequest dto)
        {
            return new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName ?? string.Empty,
                PhoneNumber = dto.PhoneNumber
            };
        }
    }
}
