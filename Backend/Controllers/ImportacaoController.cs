using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportacaoController : ControllerBase
    {
        private readonly ImportacaoService _importacaoService;

        public ImportacaoController(ImportacaoService importacaoService)
        {
            _importacaoService = importacaoService;
        }

        // POST: api/importacao/iniciar-completo
        [HttpPost("iniciar-completo")]
        public IActionResult IniciarImportacaoCompleta()
        {
            var status = _importacaoService.IniciarNovaImportacao();
            _ = _importacaoService.ProcessarUsuariosInBackgroundAsync(status.Id); // roda em background
            return Ok(new { importacaoId = status.Id });
        }

        // POST: api/importacao/iniciar-com-mudanca
        [HttpPost("iniciar-com-mudanca")]
        public IActionResult IniciarImportacaoSomenteComMudanca()
        {
            var status = _importacaoService.IniciarNovaImportacao();
            _ = _importacaoService.ProcessarUsuariosSomenteComMudancaAsync(status.Id); // roda em background
            return Ok(new { importacaoId = status.Id });
        }

        // GET: api/importacao/status/{importacaoId}
        [HttpGet("status/{importacaoId}")]
        public IActionResult GetStatus(string importacaoId)
        {
            var status = _importacaoService.GetImportacaoStatus(importacaoId);
            if (status == null)
                return NotFound(new { mensagem = "Importação não encontrada." });

            return Ok(status);
        }
    }
}
