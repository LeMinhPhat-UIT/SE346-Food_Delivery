using Messaging.Contracts.Common;
using Messaging.Contracts.Extensions;
using UserService.DTOs;
using UserService.Mappers;
using UserService.Repositories.Interfaces;
using UserService.Services.Interfaces;

namespace UserService.Services.Implements
{
    public partial class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserMapper _mapper;

        public UserService(IUserRepository userRepository, UserMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<ApiResponse<PagedResult<UserProfileResponse>>> GetAllUserAsync(PaginationRequest paginationRequest)
        {
            var userList = await _userRepository.GetAllUserAsync();
            var pagedUserList = await userList.ToPagedResultAsync(paginationRequest);

            var response = _mapper.ToUserProfileResponseList(pagedUserList.Items);

            return new ApiResponse<PagedResult<UserProfileResponse>>(
                StatusCodes.Status200OK, 
                new PagedResult<UserProfileResponse>(response));
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

        public async Task<ApiResponse<ConfirmationResponse>> UpdateUserProfileAsync(Guid id, UserProfileUpdateRequest request)
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

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);

            return new ApiResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse("Update completed successfully"));
        }
    }
}
