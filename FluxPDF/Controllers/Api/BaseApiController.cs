using Microsoft.AspNetCore.Mvc;

namespace FluxPDF.Controllers.Api
{
    [ApiController, Route("api/[controller]"), Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        protected IActionResult Success(object? data = null, string? message = null) { return Ok(new ApiResponse<object> { Success = true, Data = data, Message = message }); }
        protected IActionResult Error(string message, int statusCode = 400) { return StatusCode(statusCode, new ApiResponse<object> { Success = false, Message = message }); }
    }
    public class ApiResponse<T> { public bool Success { get; set; } public T? Data { get; set; } public string? Message { get; set; } public DateTime Timestamp { get; set; } = DateTime.UtcNow; }
}
