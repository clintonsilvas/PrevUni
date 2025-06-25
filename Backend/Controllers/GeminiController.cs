using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Text.Json;

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
            public string Dados { get; set; }
        }
        

        [HttpPost("pergunte-ia")]
        public async Task<IActionResult> PergunteIA([FromBody] PerguntaIaRequest request)
        {
            if (request == null)
                return BadRequest(new { erro = "Request inválido: corpo nulo." });

            if (string.IsNullOrEmpty(request.Prompt))
                return BadRequest(new { erro = "Prompt é obrigatório." });

            if (string.IsNullOrEmpty(request.Dados))
                return BadRequest(new { erro = "Dados do nomeCurso são obrigatórios." });

            var resposta = await _geminiService.EnviarPromptAsync(request.Prompt, request.Dados);

            if (string.IsNullOrEmpty(resposta))
            {
                return BadRequest(new { erro = "IA não respondeu." });
            }

            return Ok(new { respostaIA = resposta });
        }      



    }
}
