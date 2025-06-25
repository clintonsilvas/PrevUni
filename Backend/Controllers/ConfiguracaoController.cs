using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfiguracaoController : Controller
    {
        public class ApiKeyRequest
        {
            public string NovaChave { get; set; }
        }
        public class TextoIARequest
        {
            public string Texto { get; set; }
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

        [HttpGet("obter-texto-ia")]
        public IActionResult ObterTextoIA()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

            if (!System.IO.File.Exists(path))
                return NotFound("Arquivo de configuração não encontrado.");

            var json = System.IO.File.ReadAllText(path);
            dynamic config = JObject.Parse(json);
            string texto = config.TextGemini.ConfiguracaoIAAgente;

            return Ok(new { texto });
        }

        [HttpPost("atualizar-texto-ia")]
        public IActionResult AtualizarTextoIA([FromBody] TextoIARequest req)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");

            if (!System.IO.File.Exists(path))
                return NotFound("Arquivo de configuração não encontrado.");

            var json = System.IO.File.ReadAllText(path);
            dynamic config = JObject.Parse(json);

            if (config.TextGemini == null)
                config.TextGemini = new JObject();

            config.TextGemini.ConfiguracaoIAAgente = req.Texto;

            string atualizado = config.ToString();
            System.IO.File.WriteAllText(path, atualizado);

            return Ok();
        }
        
    }
}
