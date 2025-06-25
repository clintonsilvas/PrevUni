using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json; // Adicione este using
using System.Net.Http; // Adicione este using
using System.Threading.Tasks;

namespace Front.Pages
{
    
    public class ImportacaoInicioResponse
    {
        public string? ImportacaoId { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
    }

    
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
        [BindProperty(SupportsGet = true)]
        public string? ImportacaoId { get; set; }
        public ImportacaoStatus? CurrentStatus { get; set; } // Use ImportacaoStatus para o GET de status

        [TempData]
        public string? Message { get; set; }
        [TempData]
        public string? MessageType { get; set; }

        public async Task OnGet()
        {

            if (!string.IsNullOrEmpty(ImportacaoId))
            {
                using var client = new HttpClient();
                // Construa a URL corretamente com o ID
                var response = await client.GetAsync($"https://localhost:7232/api/Moodle/importar-status/{ImportacaoId}");

                // Verifique se a resposta foi bem-sucedida antes de tentar ler
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                CurrentStatus = JsonSerializer.Deserialize<ImportacaoStatus>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    Message = "Muitas requisi��es. Tente novamente em instantes.";
                    MessageType = "error";
                    return;
                }
                if (CurrentStatus == null)
                {
                    Message = $"Importa��o com ID '{ImportacaoId}' n�o encontrada.";
                    MessageType = "error";
                }
                else if (CurrentStatus.Status == "Conclu�da")
                {
                    Message = "Importa��o conclu�da com sucesso!";
                    MessageType = "success";
                }
                else if (CurrentStatus.Status == "Erro")
                {
                    Message = $"Erro na importa��o: {CurrentStatus.Mensagem}";
                    MessageType = "error";
                }
                else // Em Andamento, Iniciada
                {
                    Message = $"Importa��o '{CurrentStatus.Status}'. Progresso: {CurrentStatus.ProgressoAtual} de {CurrentStatus.TotalUsuarios}.";
                    MessageType = "info";
                }
            }
        }

        public async Task<IActionResult> OnPostIniciarImportacao()
        {
            using var client = new HttpClient();
            var response = await client.PostAsync("https://localhost:7232/api/Moodle/importar", new StringContent(""));

            response.EnsureSuccessStatusCode(); // Lan�a exce��o se o status n�o for sucesso

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ImportacaoInicioResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (apiResponse != null && !string.IsNullOrEmpty(apiResponse.ImportacaoId))
            {
                Message = "Importa��o iniciada com sucesso!";
                MessageType = "success";
                return RedirectToPage(new { ImportacaoId = apiResponse.ImportacaoId });
            }
            else
            {
                Message = "N�o foi poss�vel iniciar a importa��o.";
                MessageType = "error";
                return Page();
            }
        }


        public JsonResult OnGetStatusImportacao()
        {
            return new JsonResult(CurrentStatus);
        }
    }
}