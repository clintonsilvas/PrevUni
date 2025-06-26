using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text.Json;

namespace Front.Pages
{
    public class ImportacaoStatus
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public string? Mensagem { get; set; }
        public int ProgressoAtual { get; set; }
        public int TotalUsuarios { get; set; }
        public DateTime? Inicio { get; set; }
        public DateTime? Fim { get; set; }
    }

    public class ConfiguracaoModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfiguracaoModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public string? ImportacaoId { get; set; }

        public ImportacaoStatus? CurrentStatus { get; set; }

        public void OnGet()
        {
            // Nenhuma lógica específica necessária aqui para status, só passar ImportacaoId para o Razor
        }
    }
}
