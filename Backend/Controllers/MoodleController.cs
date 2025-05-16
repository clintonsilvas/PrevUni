using Microsoft.AspNetCore.Mvc;
using static Backend.MongoService;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoodleController : ControllerBase
    {
        private readonly ApiService _apiService;
        private readonly MongoService _mongoService;

        public MoodleController(ApiService apiService, MongoService mongoService)
        {
            _apiService = apiService;
            _mongoService = mongoService;
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
        [HttpPost("importar")]
        public async Task<IActionResult> ImportarDados()
        {
            try
            {
                await _apiService.ProcessarUsuariosAsync();
                return Ok("Importação concluída com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro durante a importação: {ex.Message}\n\n{ex.StackTrace}");
            }
        }
        [HttpGet("cursos")]
        public async Task<IActionResult> GetCursos()
        {
            try
            {
                var cursos = await _mongoService.GetCursosDistintosAsync();
                return Ok(cursos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar cursos: {ex.Message}");
            }
        }
        [HttpGet("cursos/com-alunos")]
        public async Task<IActionResult> GetCursosComQtdAlunos()
        {
            try
            {
                var cursos = await _mongoService.GetCursosComQtdAlunosAsync();
                return Ok(cursos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar cursos com alunos: {ex.Message}");
            }
        }

        [HttpGet("alunos-por-curso")]
        public async Task<IActionResult> GetAlunosPorCurso([FromQuery] string nomeCurso)
        {
            var alunos = await _mongoService.GetAlunosPorCursoAsync(nomeCurso);
            return Ok(alunos);
        }

        [HttpGet("acoes-por-usuario")]
        public async Task<ActionResult<List<UsuarioComAcoes>>> GetAcoesPorUsuario()
        {
            var resultado = await _mongoService.GetAcoesPorUsuarioAsync();
            return Ok(resultado);
        }


    }

}
