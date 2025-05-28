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

            //GABRIEL : Aqui você pode usar o nome do curso para buscar o resumo 
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
            var resumoResponse = await client.GetAsync($"https://localhost:7232/resumo-curso/ {CursoNome}");

            var resumoJson = await resumoResponse.Content.ReadAsStringAsync();
            var resumoObjeto = JsonSerializer.Deserialize<object>(resumoJson); // deserializa para envio

            // 2. Enviar prompt + dadosAluno para o endpoint do BACKEND (que chama o Gemini)
            var requestBody = JsonSerializer.Serialize(new
            {
                Prompt = Prompt,
                DadosCurso = resumoObjeto
            });

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var iaResponse = await client.PostAsync("https://localhost:7232/pergunte-ia", content);

            var iaResult = await iaResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(iaResult);
            Resposta = doc.RootElement.GetProperty("respostaIA").GetString();

            return Page();
        }
    }
}
