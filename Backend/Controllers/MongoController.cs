using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson; 

namespace Backend.Controllers
{
    public class MongoController : Controller
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

            return Ok(resumo.ToJson()); // Retorna em formato JSON direto
        }

        [HttpGet("resumo-aluno-ia/{userId}")]
        public async Task<IActionResult> GetResumoAlunoIA(string userId)
        {
            var resumo = await _mongoService.GerarResumoAlunoIAAsync(userId);
            if (resumo == null)
                return NotFound("Usuário não encontrado.");

            return Ok(resumo.ToJson()); // Retorna em formato JSON direto
        }
    }
}
