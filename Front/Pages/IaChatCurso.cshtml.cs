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

            var resumoResponse = await client.GetAsync($"https://localhost:7232/resumo-curso/{CursoNome}");

            var resumoJson = await resumoResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resumoJson);

            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                DadosCurso = doc.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => (object)p.Value.ToString());
            }
            else
            {
                DadosCurso = new Dictionary<string, object> {{ "dados", resumoJson }};
            }
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var resumoResponse = await client.GetAsync($"https://localhost:7232/resumo-curso/{CursoNome}");
            var resumoJson = await resumoResponse.Content.ReadAsStringAsync();

            var resumoObjeto = JsonSerializer.Deserialize<object>(resumoJson);

            var requestBody = JsonSerializer.Serialize(new
            {
                Prompt = Prompt,
                DadosCurso = resumoObjeto?.ToString() ?? "" // isso vai pegar os dados do curso
            });


            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var iaResponse = await client.PostAsync("https://localhost:7232/pergunte-ia-curso", content);

            var iaResult = await iaResponse.Content.ReadAsStringAsync();

            if (!iaResponse.IsSuccessStatusCode)
            {
                Resposta = $"Erro: {iaResponse.StatusCode} - {iaResult}";
                return Page();
            }

            using var doc = JsonDocument.Parse(iaResult);
            Resposta = doc.RootElement.GetProperty("respostaIA").GetString();

            return Page();
        }

    }
}
