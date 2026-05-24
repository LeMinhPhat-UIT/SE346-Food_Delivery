using FileService.DTOs;
using Messaging.Contracts.Common;
using System.Security.Claims;

namespace FileService.Services.Interfaces
{
    public interface IFileService
    {
        ApiResponse<PresignUrlResponse> GetUserUploadUrl(string? fileName, string? contentType, ClaimsPrincipal user);
        ApiResponse<PresignReadUrlResponse> GetUserReadUrl(string? fileKey, ClaimsPrincipal user);
        ApiResponse<PresignUrlResponse> GetDeliveryUploadUrl(Guid orderId, Guid shipperId, string? stage, string? fileName, string? contentType, ClaimsPrincipal user);
        ApiResponse<PresignReadUrlResponse> GetDeliveryReadUrl(string? fileKey, ClaimsPrincipal user);
    }
}
