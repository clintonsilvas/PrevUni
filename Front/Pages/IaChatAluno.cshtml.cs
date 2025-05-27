using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;

namespace Front.Pages
{
    public class IaChatAlunoModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IaChatAlunoModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public void OnGet()
        {
            // só carregar a página
        }

        [BindProperty]
        public string UserId { get; set; }

        [BindProperty]
        public string Prompt { get; set; }

        public string Resposta { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            
            var client = _httpClientFactory.CreateClient();

            // 1. Buscar resumo do aluno
            var resumoResponse = await client.GetAsync($"https://localhost:7232/resumo-aluno-ia/{UserId}");
            if (!resumoResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Aluno não encontrado.");
                return Page();
            }

            var resumoJson = await resumoResponse.Content.ReadAsStringAsync();
            var resumoObjeto = JsonSerializer.Deserialize<object>(resumoJson); // deserializa para envio

            // 2. Enviar prompt + dadosAluno para o endpoint do BACKEND (que chama o Gemini)
            var requestBody = JsonSerializer.Serialize(new
            {
                Prompt = Prompt,
                DadosAluno = resumoObjeto
            });

            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var iaResponse = await client.PostAsync("https://localhost:7232/pergunte-ia", content);
            if (!iaResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Erro ao consultar IA.");
                return Page();
            }

            var iaResult = await iaResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(iaResult);
            Resposta = doc.RootElement.GetProperty("respostaIA").GetString();

            return Page();
        }

    }
}
