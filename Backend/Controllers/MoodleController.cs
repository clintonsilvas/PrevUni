using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoodleController : ControllerBase
    {
        private readonly ApiService _apiService;

        public MoodleController(ApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            var result = await _apiService.GetUsuarios();
            return Ok(result);
        }

        [HttpGet("logs/{userId}")]
        public async Task<IActionResult> GetLogs(string userId)
        {
            var result = await _apiService.GetLogs(userId);
            return Ok(result);
        }
    }

}
