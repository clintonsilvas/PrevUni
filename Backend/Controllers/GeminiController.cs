using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    public class GeminiController : Controller
    {
        private readonly GeminiService _geminiService;

        public GeminiController(GeminiService geminiService)
        {
            _geminiService = geminiService;
        }
        public class PerguntaIaRequest
        {
            public string Prompt { get; set; }
            public string DadosAluno { get; set; }
        }
        [HttpPost("pergunte-ia")]
        public async Task<IActionResult> PergunteIA([FromBody] PerguntaIaRequest request)
        {
            var resposta = await _geminiService.EnviarPromptAsync(request.Prompt, request.DadosAluno);
            if (string.IsNullOrEmpty(resposta))
                return NotFound("IA não respondeu.");

            return Ok(new { respostaIA = resposta });
        }


        public class PerguntaIaCursoRequest
        {
            public string Prompt { get; set; }
            public string DadosCurso { get; set; }
        }

        [HttpPost("pergunte-ia-curso")]
        public async Task<IActionResult> PergunteIACurso([FromBody] PerguntaIaCursoRequest request)
        {
            if (request == null)
                return BadRequest(new { erro = "Request inválido: corpo nulo." });

            if (string.IsNullOrEmpty(request.Prompt))
                return BadRequest(new { erro = "Prompt é obrigatório." });

            if (string.IsNullOrEmpty(request.DadosCurso))
                return BadRequest(new { erro = "Dados do curso são obrigatórios." });

            var resposta = await _geminiService.EnviarPromptAsync(request.Prompt, request.DadosCurso);

            if (string.IsNullOrEmpty(resposta))
            {
                return BadRequest(new { erro = "IA não respondeu." });
            }

            return Ok(new { respostaIA = resposta });
        }


    }
}
