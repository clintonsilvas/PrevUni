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

        public MongoController(MongoService mongoService)
        {
            _mongoService = mongoService;
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

        [HttpGet("resumo-curso/{nomeCurso}")]
        public async Task<IActionResult> ResumoCurso(string nomeCurso)
        {
            var resumo = await _mongoService.GerarResumoCursoIAAsync(nomeCurso);
            return Ok(new { resumo });
        }

        [HttpGet("usuarios-com-ultimo-acesso")]
        public async Task<IActionResult> GetUsuariosComUltimoAcesso()
        {
            var usuarios = await _mongoService.GetUsuariosComUltimoAcessoAsync();
            return Ok(usuarios);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class EngajamentoController : ControllerBase
    {
        private readonly EngajamentoService _engajamentoService;

        public EngajamentoController(EngajamentoService engajamentoService)
        {
            _engajamentoService = engajamentoService;
        }

        // GET api/engajamento/curso/{nomeCurso}
        [HttpGet("curso/{nomeCurso}")]
        public async Task<IActionResult> GetEngajamentoAlunosPorCurso(string nomeCurso)
        {
            var lista = await _engajamentoService.CalcularEngajamentoAlunosPorCursoAsync(nomeCurso);
            return Ok(lista);
        }
    }
}
