using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using static Backend.Services.MongoService;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoodleController : ControllerBase
    {
        
        private readonly MongoService _mongoService;

        public MoodleController(UnifenasService apiService, MongoService mongoService)
        {            
            _mongoService = mongoService;
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

        [HttpGet("alunos-por-curso/{nomeCurso}")]
        public async Task<IActionResult> GetAlunosPorCurso(string nomeCurso)
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
        [HttpGet("logs-por-nomeCurso/{nomeCurso}")]
        public async Task<IActionResult> GetLogsPorCurso(string nomeCurso)
        {
            try
            {
                var logs = await _mongoService.GetLogsPorCursoAsync(nomeCurso);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar logs: {ex.Message}");
            }
        }

        [HttpGet("curso/alunos")]
        public async Task<IActionResult> GetAlunosPorTodosOsCursos()
        {
            try
            {
                var logs = await _mongoService.GetAlunosPorTodosOsCursosAsync();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar logs: {ex.Message}");
            }
        }

    }

}
