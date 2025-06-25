using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

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
            public int pos { get; set; }
        }
        [HttpPost("pergunte-ia")]
        public async Task<IActionResult> PergunteIA([FromBody] PerguntaIaRequest request)
        {
            var resposta = await _geminiService.EnviarPromptAsync(request.Prompt, request.DadosAluno, request.pos);
            if (string.IsNullOrEmpty(resposta))
                return NotFound("IA não respondeu.");

            return Ok(new { respostaIA = resposta });
        }


        public class PerguntaIaCursoRequest
        {
            public string Prompt { get; set; }
            public string DadosCurso { get; set; }
        }

        [HttpPost("pergunte-ia-nomeCurso")]
        public async Task<IActionResult> PergunteIACurso([FromBody] PerguntaIaCursoRequest request)
        {
            if (request == null)
                return BadRequest(new { erro = "Request inválido: corpo nulo." });

            if (string.IsNullOrEmpty(request.Prompt))
                return BadRequest(new { erro = "Prompt é obrigatório." });

            if (string.IsNullOrEmpty(request.DadosCurso))
                return BadRequest(new { erro = "Dados do nomeCurso são obrigatórios." });

            var resposta = await _geminiService.EnviarPromptAsync(request.Prompt, request.DadosCurso, 0);

            if (string.IsNullOrEmpty(resposta))
            {
                return BadRequest(new { erro = "IA não respondeu." });
            }

            return Ok(new { respostaIA = resposta });
        }
        public class ApiKeyRequest
        {
            public string NovaChave { get; set; }
        }
        [HttpPost("atualizar-api-key")]
        public IActionResult AtualizarApiKey([FromBody] ApiKeyRequest req)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

            var json = System.IO.File.ReadAllText(path);
            dynamic config = JObject.Parse(json);

            config.Gemini.ApiKey = req.NovaChave;

            string atualizado = config.ToString();
            System.IO.File.WriteAllText(path, atualizado);

            return Ok();
        }
        [HttpGet("obter-api-key")]
        public IActionResult ObterApiKey()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

            if (!System.IO.File.Exists(path))
                return NotFound("Arquivo de configuração não encontrado.");

            var json = System.IO.File.ReadAllText(path);
            dynamic config = JObject.Parse(json);
            string chave = config.Gemini.ApiKey;

            // Por segurança, envie mascarada (ou toda se quiser)
            return Ok(new { chave = chave });
        }


    }
}
