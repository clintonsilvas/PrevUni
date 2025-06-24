using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Front.Pages
{
    public class IaChatCursoModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IaChatCursoModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /* --------------------------------------------------
         * Bindings vindos da rota/formulário
         * -------------------------------------------------*/
        [BindProperty(SupportsGet = true)]
        public string CursoNome { get; set; } = string.Empty;

        [BindProperty]
        public string Prompt { get; set; } = string.Empty;

        /* --------------------------------------------------
         * Dados expostos à view
         * -------------------------------------------------*/
        public string Resposta { get; set; } = string.Empty;
        public Dictionary<string, JsonElement>? DadosCurso { get; set; }

        /* ==================================================
         * GET: carrega resumo para exibir na tela
         * =================================================*/
        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();

            try
            {
                var resumoJson = await client.GetStringAsync(
                    $"https://localhost:7232/api/Mongo/resumo-curso/{CursoNome}");

                using var doc = JsonDocument.Parse(resumoJson);

                // Se for objeto, converte em dicionário para renderizar na view
                if (doc.RootElement.ValueKind == JsonValueKind.Object)
                {
                    DadosCurso = doc.RootElement.EnumerateObject()
                        .ToDictionary(p => p.Name, p => p.Value);
                }
                else
                {
                    DadosCurso = new Dictionary<string, JsonElement>
                    {
                        { "dados", doc.RootElement }
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                DadosCurso = new()
                {
                    { "erro", JsonDocument.Parse($"\"Erro de rede: {ex.Message}\"").RootElement }
                };
            }
            catch (JsonException ex)
            {
                DadosCurso = new()
                {
                    { "erro", JsonDocument.Parse($"\"Erro ao processar JSON: {ex.Message}\"").RootElement }
                };
            }
        }

        /* ==================================================
         * POST: envia pergunta + resumo para a IA
         * =================================================*/
        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient();

            string resumoJson;
            try
            {
                resumoJson = await client.GetStringAsync(
                    $"https://localhost:7232/api/Mongo/resumo-curso/{CursoNome}");
            }
            catch (HttpRequestException ex)
            {
                Resposta = $"Erro ao buscar resumo: {ex.Message}";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(resumoJson))
            {
                Resposta = "Erro: o resumo veio vazio.";
                return Page();
            }

            JsonDocument resumoDoc;
            try
            {
                resumoDoc = JsonDocument.Parse(resumoJson);
            }
            catch (JsonException ex)
            {
                Resposta = $"Erro ao interpretar resumo: {ex.Message}";
                return Page();
            }

            var requestBody = JsonSerializer.Serialize(new
            {
                Prompt,
                DadosCurso = resumoDoc.RootElement
            });

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            HttpResponseMessage iaResponse;
            try
            {
                iaResponse = await client.PostAsync("https://localhost:7232/pergunte-ia-nomeCurso", content);
            }
            catch (HttpRequestException ex)
            {
                Resposta = $"Erro ao chamar IA: {ex.Message}";
                return Page();
            }

            var iaResult = await iaResponse.Content.ReadAsStringAsync();

            if (!iaResponse.IsSuccessStatusCode)
            {
                Resposta = $"Erro: {iaResponse.StatusCode} - {iaResult}";
                return Page();
            }

            try
            {
                using var doc = JsonDocument.Parse(iaResult);
                Resposta = doc.RootElement.GetProperty("respostaIA").GetString() ?? string.Empty;
            }
            catch (JsonException ex)
            {
                Resposta = $"Erro ao interpretar resposta da IA: {ex.Message}";
            }

            return Page();
        }

    }
}
