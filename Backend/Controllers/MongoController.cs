using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Generic;
using static Backend.Services.MongoService;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MongoController : ControllerBase
    {
        private readonly MongoService _mongoService;
        private readonly EngajamentoService _engajamentoService;

        public MongoController(MongoService mongoService, EngajamentoService engajamentoService)
        {
            _mongoService = mongoService;
            _engajamentoService = engajamentoService;
        }

        [HttpGet("resumo-aluno/{userId}")]
        public async Task<IActionResult> GetResumoAluno(string userId)
        {
            var resumo = await _mongoService.GerarResumoAlunoAsync(userId);
            if (resumo == null)
                return NotFound("Usuário não encontrado.");

            return Ok(resumo.ToJson());
        }

        [HttpGet("resumo-aluno-ia/{userId}")]
        public async Task<IActionResult> GetResumoAlunoIA(string userId)
        {
            var resumo = await _mongoService.GerarResumoAlunoIAAsync(userId);
            if (resumo == null)
                return NotFound("Usuário não encontrado.");

            return Ok(resumo.ToJson());
        }

        [HttpGet("resumo-nomeCurso/{nomeCurso}")]
        public async Task<IActionResult> ResumoCurso(string nomeCurso)
        {
            var resumo = await _mongoService.GerarResumoCursoIAAsync(nomeCurso);
            return Ok(new { resumo });
        }

        // Engajamento de todos os alunos
        [HttpGet("engajamento-alunos")]
        public async Task<IActionResult> GetEngajamentoAlunos()
        {
            var lista = await _engajamentoService.CalcularEngajamentoAlunosAsync();
            return Ok(lista);
        }

        [HttpGet("usuarios-com-ultimo-acesso")]
        public async Task<IActionResult> GetUsuariosComUltimoAcesso()
        {
            var usuarios = await _mongoService.GetUsuariosComUltimoAcessoAsync();
            return Ok(usuarios);
        }


    }
}
