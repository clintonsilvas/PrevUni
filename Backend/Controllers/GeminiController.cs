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



    }
}
