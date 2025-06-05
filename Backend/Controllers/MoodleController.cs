using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using static Backend.Services.MongoService;

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
        public IActionResult IniciarImportacao() // Note que não é mais async Task<IActionResult>
        {
            try
            {
                // Inicia o registro da importação e obtém o ID
                var statusInicial = _apiService.IniciarNovaImportacao();

                // Inicia a tarefa de processamento em background, passando o ID
                // O underscore é para "descartar" o resultado da Task, indicando "fire and forget".
                // Isso não significa que a Task não será executada, apenas que não estamos esperando por ela aqui.
                _ = Task.Run(() => _apiService.ProcessarUsuariosSomenteComMudancaAsync(statusInicial.Id));

                // Retorna imediatamente um status 202 Accepted (Processando)
                return Accepted(new { importacaoId = statusInicial.Id, status = statusInicial.Status, message = statusInicial.Mensagem });
            }
            catch (Exception ex)
            {
                // Retornar um erro genérico se a inicialização falhar
                return StatusCode(500, $"Erro ao iniciar a importação: {ex.Message}");
            }
        }

        [HttpGet("importar-status/{importacaoId}")]
        public IActionResult GetStatusImportacao(string importacaoId)
        {
            var status = _apiService.GetImportacaoStatus(importacaoId);

            if (status == null)
            {
                return NotFound($"Importação com ID '{importacaoId}' não encontrada.");
            }

            return Ok(status);
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
        [HttpGet("logs-por-curso/{nomeCurso}")]
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
