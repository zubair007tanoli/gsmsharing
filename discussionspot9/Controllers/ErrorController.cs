using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace discussionspot9.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error")]
        [Route("Error/{statusCode}")]
        public IActionResult Index(int? statusCode = null)
        {
            var errorId = Guid.NewGuid().ToString();
            ViewData["ErrorId"] = errorId;

            // Get exception details if available
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            
            if (exceptionFeature != null)
            {
                _logger.LogError(exceptionFeature.Error, 
                    $"Error {errorId} at {exceptionFeature.Path}");
            }

            if (statusCode.HasValue)
            {
                switch (statusCode.Value)
                {
                    case 404:
                        _logger.LogWarning($"404 Error: {HttpContext.Request.Path} - ErrorId: {errorId}");
                        return View("Error404");
                    
                    case 500:
                    case 503:
                        _logger.LogError($"5xx Error {statusCode}: {HttpContext.Request.Path} - ErrorId: {errorId}");
                        return View("Error500");
                    
                    default:
                        return View("Error");
                }
            }

            return View("Error500");
        }

        [Route("404")]
        public IActionResult NotFound()
        {
            Response.StatusCode = 404;
            _logger.LogWarning($"404: {HttpContext.Request.Path}");
            return View("Error404");
        }
    }
}

