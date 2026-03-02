using Microsoft.AspNetCore.Mvc;

namespace Rag.Services.Backend.Api.Controllers
{
    public class HomeController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult Get()
            => Content("Rag.Services.Backend is working!");
    }
}