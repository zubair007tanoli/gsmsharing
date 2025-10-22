using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using discussionspot9.Services;

namespace discussionspot9.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IMediaUploadService _mediaUploadService;
        private readonly ILogger<MediaController> _logger;

        public MediaController(IMediaUploadService mediaUploadService, ILogger<MediaController> logger)
        {
            _mediaUploadService = mediaUploadService;
            _logger = logger;
        }

        [HttpPost("upload")]
        [Authorize]
        public async Task<IActionResult> UploadFile(IFormFile file, string category = "stories")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, errorMessage = "No file provided." });
                }

                var result = await _mediaUploadService.UploadFileAsync(file, category);
                
                if (result.Success)
                {
                    return Ok(new
                    {
                        success = true,
                        filePath = result.FilePath,
                        fileName = result.FileName,
                        fileSize = result.FileSize,
                        mediaType = result.MediaType,
                        mediaInfo = result.MediaInfo
                    });
                }
                else
                {
                    return BadRequest(new { success = false, errorMessage = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { success = false, errorMessage = "An error occurred while uploading the file." });
            }
        }

        [HttpDelete("{*filePath}")]
        [Authorize]
        public async Task<IActionResult> DeleteFile(string filePath)
        {
            try
            {
                var success = await _mediaUploadService.DeleteFileAsync(filePath);
                
                if (success)
                {
                    return Ok(new { success = true, message = "File deleted successfully." });
                }
                else
                {
                    return NotFound(new { success = false, errorMessage = "File not found." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file: {filePath}");
                return StatusCode(500, new { success = false, errorMessage = "An error occurred while deleting the file." });
            }
        }

        [HttpGet("info/{*filePath}")]
        public async Task<IActionResult> GetFileInfo(string filePath)
        {
            try
            {
                var mediaInfo = await _mediaUploadService.GetMediaInfoAsync(filePath);
                
                if (mediaInfo != null)
                {
                    return Ok(new { success = true, mediaInfo });
                }
                else
                {
                    return NotFound(new { success = false, errorMessage = "File not found." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting file info: {filePath}");
                return StatusCode(500, new { success = false, errorMessage = "An error occurred while getting file information." });
            }
        }
    }
}
