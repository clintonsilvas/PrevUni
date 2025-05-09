using Microsoft.AspNetCore.Mvc;
using PrevUni.Services;

namespace PrevUni.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CursoController : ControllerBase
    {
        private readonly AlunoService _alunoService;

        public CursoController(AlunoService alunoService)
        {
            _alunoService = alunoService;
        }

        [HttpGet]
        public async Task<IActionResult> ObterCursos()
        {
            var alunos = await _alunoService.ListarAlunosAsync();

            var cursos = alunos
                .GroupBy(a => new { a.Curso, a.NomeCurso, a.CoordenadorCurso })
                .Select(g => new
                {
                    curso = g.Key.Curso,
                    nomeCurso = g.Key.NomeCurso,
                    coordenador = g.Key.CoordenadorCurso
                })
                .Distinct()
                .ToList();

            return Ok(cursos);
        }
    }
}
