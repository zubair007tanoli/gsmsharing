using Microsoft.AspNetCore.Mvc;

namespace FluxPDF.Controllers.Api
{
    [Route("api/[controller]"), ApiController]
    public class HealthController : BaseApiController
    {
        [HttpGet]
        public IActionResult Get() { return Success(new { Status = "Healthy", Version = "1.0.0", Timestamp = DateTime.UtcNow }); }
    }
}
