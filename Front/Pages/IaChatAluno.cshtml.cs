using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using System.Globalization;
using Front.Models;

namespace Front.Pages
{
    public class IaChatAlunoModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        public IaChatAlunoModel(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        [BindProperty]
        public string UserId { get; set; }
        [BindProperty]
        public string Prompt { get; set; } = string.Empty;
        public List<MensagemIA> Historico { get; set; } = new();

        public class MensagemIA
        {
            public string Pergunta { get; set; }
            public string Resposta { get; set; }
        }

        public async Task OnGetAsync()
        {
            if (TempData["Historico"] is string historicoJson)
                Historico = JsonSerializer.Deserialize<List<MensagemIA>>(historicoJson) ?? new();
        }        

        public async Task<IActionResult> OnPostAsync()
        {
            if (TempData["Historico"] is string historicoJson)
                Historico = JsonSerializer.Deserialize<List<MensagemIA>>(historicoJson) ?? new();
            else
                Historico = new();

            var client = _httpClientFactory.CreateClient();

            try
            {
                var resumoJson = await client.GetStringAsync($"{_configuration["ApiUrl"]}/api/Mongo/resumo-aluno-ia/{UserId}");
                var resumoTexto = JsonDocument.Parse(resumoJson).RootElement.GetProperty("resumo").GetString();

                var requestBody = JsonSerializer.Serialize(new
                {
                    Prompt,
                    Dados = resumoTexto
                });

                var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                var iaResponse = await client.PostAsync($"{_configuration["ApiUrl"]}/pergunte-ia", content);
                var iaResult = await iaResponse.Content.ReadAsStringAsync();

                var resposta = "IA não respondeu.";
                if (iaResponse.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(iaResult);
                    resposta = doc.RootElement.GetProperty("respostaIA").GetString() ?? resposta;
                }

                Historico.Add(new MensagemIA { Pergunta = Prompt, Resposta = resposta });
            }
            catch
            {
                Historico.Add(new MensagemIA
                {
                    Pergunta = Prompt,
                    Resposta = "⚠️ Ocorreu um erro ao processar sua pergunta. Tente novamente."
                });
            }

            TempData["Historico"] = JsonSerializer.Serialize(Historico);
            Prompt = string.Empty;
            return RedirectToPage();
        }  
    }
}
