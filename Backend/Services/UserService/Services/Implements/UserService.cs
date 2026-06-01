using Messaging.Contracts.Common;
using Messaging.Contracts.Extensions;
using Messaging.RabbitMq.Publishing;
using UserService.DTOs.User;
using UserService.Mappers;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implements
{
    public partial class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly UserMapper _mapper;

        public UserService(IUserRepository userRepository, IEventPublisher eventPublisher, UserMapper mapper)
        {
            _userRepository = userRepository;
            _eventPublisher = eventPublisher;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResult<UserProfileResponse>>> GetAllUserAsync(PaginationRequest paginationRequest)
        {
            var userList = await _userRepository.GetAllUserAsync();
            var pagedUserList = await userList.ToPagedResultAsync(paginationRequest);

            var response = _mapper.ToUserProfileResponseList(pagedUserList.Items);
            var result = new PagedResult<UserProfileResponse>(response, pagedUserList.PaginationRequest, pagedUserList.TotalCount);

            return new ApiResponse<PagedResult<UserProfileResponse>>(
                StatusCodes.Status200OK,
                result);
        }

        public async Task<ApiResponse<UserProfileResponse>> GetUserAsync(Guid id)
        {
            if (id == Guid.Empty)
                return new ApiResponse<UserProfileResponse>(StatusCodes.Status400BadRequest, "Invalid user id");

            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
                return new ApiResponse<UserProfileResponse>(StatusCodes.Status404NotFound, "No user found");

            var response = _mapper.ToUserProfileResponse(user);

            return new ApiResponse<UserProfileResponse>(StatusCodes.Status200OK, response);
        }

        public async Task<ApiResponse<ConfirmationResponse>> DeleteUserAsync(Guid id)
        {
            if (id == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid user id");

            var isDeleted = await _userRepository.DeleteUserAsync(id);

            if (!isDeleted)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No user found");

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Delete completed successfully"));
        }

        public async Task<ApiResponse<ConfirmationResponse>> UpdateUserProfileAsync(Guid id, UpdateUserProfileRequest request)
        {
            if (id == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid user id");

            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No user found");

            if (!string.IsNullOrWhiteSpace(request.FullName))
                user.FullName = request.FullName;

            if (!string.IsNullOrWhiteSpace(request.AvatarUrl))
                user.AvatarUrl = request.AvatarUrl;

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Update completed successfully"));
        }

        public async Task<ApiResponse<PagedResult<UserAddressResponse>>> GetAllUserAddressesAsync(PaginationRequest paginationRequest)
        {
            var addresses = await _userRepository.GetAllUserAddressesAsync();

            var pagedAddresses = await addresses.ToPagedResultAsync(paginationRequest);

            var response = _mapper.ToUserAddressResponseList(pagedAddresses.Items);
            var result = new PagedResult<UserAddressResponse>(response, pagedAddresses.PaginationRequest, pagedAddresses.TotalCount);

            return new ApiResponse<PagedResult<UserAddressResponse>>(StatusCodes.Status200OK, result);
        }

        public async Task<ApiResponse<ConfirmationResponse>> AddUserAddressAsync(Guid userId, CreateUserAddressRequest request)
        {
            if (userId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid user id");

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No user found");

            var address = _mapper.ToAddress(request);
            address.UserId = userId;
            address.CreatedAt = DateTime.UtcNow;

            var result = await _userRepository.CreateUserAddressAsync(address);

            if (!result)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Can not add address");

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Address added succesfully"));
        }

        public async Task<ApiResponse<ConfirmationResponse>> UpdateUserAddressAsync(Guid addressId, UpdateUserAddressRequest request)
        {
            if (addressId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid address id");

            var address = await _userRepository.GetUserAddressByIdAsync(addressId);

            if (address == null)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No address found");

            if (!string.IsNullOrEmpty(request.Ward))
                address.Ward = request.Ward;

            if (!string.IsNullOrEmpty(request.District))
                address.District = request.District;

            if (!string.IsNullOrEmpty(request.City))
                address.City = request.City;

            if (request.Lat != null)
                address.Lat = request.Lat;

            if (request.Lng != null)
                address.Lng = request.Lng;

            if (!string.IsNullOrEmpty(request.Label))
                address.Label = request.Label;

            if (!string.IsNullOrEmpty(request.RecipientName))
                address.RecipientName = request.RecipientName;

            if (!string.IsNullOrEmpty(request.Phone))
                address.Phone = request.Phone;

            address.AddressLine = request.AddressLine;
            address.IsDefault = request.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAddressAsync(address);

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Address updated successfully"));
        }

        public async Task<ApiResponse<PagedResult<UserAddressResponse>>> GetAllUserAddressesByUserIdAsync(Guid userId, PaginationRequest paginationRequest)
        {
            if (userId == Guid.Empty)
                return new ApiResponse<PagedResult<UserAddressResponse>>(StatusCodes.Status400BadRequest, "Invalid user id");

            var addresses = await _userRepository.GetAllUserAddressesByUserIdAsync(userId);

            if (addresses == null)
                return new ApiResponse<PagedResult<UserAddressResponse>>(StatusCodes.Status404NotFound, "No user found");

            var pagedAddresses = await addresses.ToPagedResultAsync(paginationRequest);

            var response = _mapper.ToUserAddressResponseList(pagedAddresses.Items);
            var result = new PagedResult<UserAddressResponse>(response, pagedAddresses.PaginationRequest, pagedAddresses.TotalCount);

            return new ApiResponse<PagedResult<UserAddressResponse>>(StatusCodes.Status200OK, result);
        }

        public async Task<ApiResponse<UserAddressResponse>> GetUserAddressByIdAsync(Guid addressId)
        {
            if (addressId == Guid.Empty)
                return new ApiResponse<UserAddressResponse>(StatusCodes.Status400BadRequest, "Invalid address id");

            var address = await _userRepository.GetUserAddressByIdAsync(addressId);

            if (address == null)
                return new ApiResponse<UserAddressResponse>(StatusCodes.Status404NotFound, "No address found");

            var response = _mapper.ToUserAddressResponse(address);

            return new ApiResponse<UserAddressResponse>(StatusCodes.Status200OK, response);
        }

        public async Task<ApiResponse<ConfirmationResponse>> DeleteUserAddressAsync(Guid addressId)
        {
            if (addressId == Guid.Empty)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest, "Invalid address id");

            var isDeleted = await _userRepository.DeleteUserAddressAsync(addressId);

            if (!isDeleted)
                return new ApiResponse<ConfirmationResponse>(StatusCodes.Status404NotFound, "No address found");

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Address deleted successfully"));
        }
    }
}
