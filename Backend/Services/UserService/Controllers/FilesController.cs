using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.DTOs;
using UserService.Helpers;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly FirebaseStorageHelper _storageHelper;

        public FilesController(FirebaseStorageHelper storageHelper)
        {
            _storageHelper = storageHelper;
        }

        [HttpGet("get-upload-url")]
        [Authorize]
        public ActionResult<ApiResponse<PresignUrlResponse>> GetUploadUrl([FromQuery] string fileName, [FromQuery] string contentType)
        {
            var extension = Path.GetExtension(fileName);
            string newFileName = $"{Guid.NewGuid()}{extension}";

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
            string filePath = $"users/{userId}/{newFileName}";

            var uploadUrl = _storageHelper.GenerateUploadUrl(filePath, contentType);

            return Ok(new ApiResponse<PresignUrlResponse>
            {
                Data = new PresignUrlResponse
                {
                    FileKey = filePath, 
                    UploadUrl = uploadUrl,
                    ContentType = contentType
                }
            });
        }
    }
}
