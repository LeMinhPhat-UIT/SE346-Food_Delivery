using FileService.Services.Interfaces;
using FileService.DTOs;
using Messaging.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("get-upload-url")]
        public ActionResult<ApiResponse<PresignUrlResponse>> GetUserUploadUrl([FromQuery] string fileName, [FromQuery] string contentType)
        {
            try
            {
                var response = _fileService.GetUserUploadUrl(fileName, contentType, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("get-read-url")]
        public ActionResult<ApiResponse<PresignReadUrlResponse>> GetUserReadUrl([FromQuery] string? fileKey)
        {
            try
            {
                var response = _fileService.GetUserReadUrl(fileKey, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("/api/deliveries/files/upload-url")]
        public ActionResult<ApiResponse<PresignUrlResponse>> GetDeliveryUploadUrl([FromQuery] Guid orderId, [FromQuery] Guid shipperId, [FromQuery] string stage, [FromQuery] string fileName, [FromQuery] string contentType)
        {
            try
            {
                var response = _fileService.GetDeliveryUploadUrl(orderId, shipperId, stage, fileName, contentType, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("/api/deliveries/files/read-url")]
        public ActionResult<ApiResponse<PresignReadUrlResponse>> GetDeliveryReadUrl([FromQuery] string? fileKey)
        {
            try
            {
                var response = _fileService.GetDeliveryReadUrl(fileKey, User);

                if (!response.Success)
                    return StatusCode(response.StatusCode, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
