using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        [BindProperty(SupportsGet = true)]
        public string CursoNome { get; set; }
        [BindProperty]
        public string Prompt { get; set; }

        public string Resposta { get; set; }

        public Dictionary<string, object> DadosCurso { get; set; }

        public async Task OnGetAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var resumoResponse = await client.GetAsync($"https://localhost:7232/api/Mongo/resumo-nomeCurso/{CursoNome}");


            if (!resumoResponse.IsSuccessStatusCode)
            {
                DadosCurso = new Dictionary<string, object> {
            { "erro", $"Erro na requisição: {resumoResponse.StatusCode}" }
        };
                return;
            }

            var resumoJson = await resumoResponse.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(resumoJson))
            {
                try
                {
                    using var doc = JsonDocument.Parse(resumoJson);

                    if (doc.RootElement.ValueKind == JsonValueKind.Object)
                    {
                        DadosCurso = doc.RootElement.EnumerateObject()
                            .ToDictionary(p => p.Name, p => (object)p.Value.ToString());
                    }
                    else
                    {
                        DadosCurso = new Dictionary<string, object> { { "dados", resumoJson } };
                    }
                }
                catch (JsonException ex)
                {
                    DadosCurso = new Dictionary<string, object> {
                { "erro", $"Erro ao processar JSON: {ex.Message}" }
            };
                }
            }
            else
            {
                DadosCurso = new Dictionary<string, object> {
            { "erro", "Resposta da API está vazia." }
        };
            }
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var resumoResponse = await client.GetAsync($"https://localhost:7232/resumo-nomeCurso/{CursoNome}");
            var resumoJson = await resumoResponse.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(resumoJson))
            {
                Resposta = "Erro: a resposta do resumo veio vazia.";
                return Page();
            }

            object resumoObjeto;

            try
            {
                resumoObjeto = JsonSerializer.Deserialize<object>(resumoJson);
            }
            catch (JsonException ex)
            {
                Resposta = $"Erro ao processar o resumo JSON: {ex.Message}";
                return Page();
            }

            var requestBody = JsonSerializer.Serialize(new
            {
                Prompt = Prompt,
                DadosCurso = resumoObjeto?.ToString() ?? ""
            });

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var iaResponse = await client.PostAsync("https://localhost:7232/pergunte-ia-nomeCurso", content);
            var iaResult = await iaResponse.Content.ReadAsStringAsync();

            if (!iaResponse.IsSuccessStatusCode)
            {
                Resposta = $"Erro: {iaResponse.StatusCode} - {iaResult}";
                return Page();
            }

            try
            {
                using var doc = JsonDocument.Parse(iaResult);
                Resposta = doc.RootElement.GetProperty("respostaIA").GetString();
            }
            catch (JsonException ex)
            {
                Resposta = $"Erro ao interpretar resposta da IA: {ex.Message}";
            }

            return Page();
        }


    }
}
