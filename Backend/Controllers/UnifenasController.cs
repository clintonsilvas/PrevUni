using Backend.Services;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnifenasController : ControllerBase
    {
        private readonly UnifenasService _unifenasService;

        public UnifenasController(UnifenasService unifenasService)
        {
            _unifenasService = unifenasService;
        }

        // GET: api/unifenas/usuarios
        [HttpGet("usuarios")]
        public async Task<ActionResult<List<Usuario>>> GetUsuarios()
        {
            try
            {
                var usuarios = await _unifenasService.GetUsuariosAsync();
                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar usuários: {ex.Message}");
            }
        }

        // GET: api/unifenas/logs/{userId}
        [HttpGet("logs/{userId}")]
        public async Task<ActionResult<List<LogUsuario>>> GetLogs(string userId)
        {
            try
            {
                var logs = await _unifenasService.GetLogsAsync(userId);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar logs: {ex.Message}");
            }
        }
    }
}
