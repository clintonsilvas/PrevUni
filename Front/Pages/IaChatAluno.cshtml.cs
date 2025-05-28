using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;
using Front.Models;

namespace Front.Pages
{
    public class IaChatAlunoModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public IaChatAlunoModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }
       // public Usuario user { get; set; }

        [BindProperty]
        public string Prompt { get; set; }

        public string Resposta { get; set; }
        public string RespostaFormatada
        {
            get { return ConverterParaHtml(Resposta); }
        }
        public string UltimaPergunta { get; set; }

        public List<MensagemChat> Mensagens = new List<MensagemChat>();

        public Dictionary<string, object> DadosAluno { get; set; }

        public async Task OnGetAsync()
        {
            await CarregarDadosAluno();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var client = _httpClientFactory.CreateClient();

            // Buscar dados do aluno
            var resumoResponse = await client.GetAsync($"https://localhost:7232/resumo-aluno-ia/{UserId}");
            if (!resumoResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Aluno não encontrado.");
                return Page();
            }

            var resumoJson = await resumoResponse.Content.ReadAsStringAsync();
            var resumoObjeto = JsonSerializer.Deserialize<object>(resumoJson);

            // Enviar para IA
            var requestBody = JsonSerializer.Serialize(new
            {
                Prompt,
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
            UltimaPergunta = Prompt;
            Prompt = string.Empty;
            await CarregarDadosAluno();
            return Page();
        }
        private async Task CarregarDadosAluno()
        {
            var client = _httpClientFactory.CreateClient();
            var resumoResponse = await client.GetAsync($"https://localhost:7232/resumo-aluno-ia/{UserId}");

            if (!resumoResponse.IsSuccessStatusCode)
            {
                DadosAluno = new();
                return;
            }

            var resumoJson = await resumoResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resumoJson);

            if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                DadosAluno = doc.RootElement.EnumerateObject()
                    .ToDictionary(p => p.Name, p => (object)p.Value.ToString());
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.String)
            {
                var dadosString = doc.RootElement.GetString();
                var pares = dadosString.Split(" / ");

                DadosAluno = pares.Select(par =>
                {
                    var partes = par.Split(": ", 2);
                    return new
                    {
                        Chave = partes[0].Trim(),
                        Valor = partes.Length > 1 ? partes[1].Trim() : ""
                    };
                }).ToDictionary(p => p.Chave, p => (object)p.Valor);
            }
            else
            {
                DadosAluno = new Dictionary<string, object>
        {
            { "dados", resumoJson }
        };
            }
        }
        public string ConverterParaHtml(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return "";

            // Negrito markdown **texto** -> <strong>texto</strong>
            texto = System.Text.RegularExpressions.Regex.Replace(
                texto, @"\*\*(.+?)\*\*", "<strong>$1</strong>");

            var paragrafos = texto.Split(new string[] { "\n\n" }, StringSplitOptions.None);

            var sb = new StringBuilder();

            for (int i = 0; i < paragrafos.Length; i++)
            {
                var linha = paragrafos[i].Trim();

                if (linha.StartsWith("*"))
                {
                    var linhasLista = linha.Split('\n').Select(l => l.Trim()).ToList();

                    sb.Append("<ul>");
                    foreach (var item in linhasLista)
                    {
                        if (item.StartsWith("*"))
                        {
                            var conteudo = item.Substring(1).Trim();
                            sb.Append($"<li>{conteudo}</li>");
                        }
                    }
                    sb.Append("</ul>");
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(linha, @"^\d+\.")) // lista numerada
                {
                    var linhasLista = linha.Split('\n').Select(l => l.Trim()).ToList();
                    sb.Append("<ol>");
                    foreach (var item in linhasLista)
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(item, @"^\d+\.\s*(.+)");
                        if (match.Success)
                        {
                            sb.Append($"<li>{match.Groups[1].Value}</li>");
                        }
                    }
                    sb.Append("</ol>");
                }
                else
                {
                    // Quebra simples vira <br>
                    var textoComBr = linha.Replace("\n", "<br>");
                    sb.Append(textoComBr);
                }

                // Adiciona dupla quebra <br><br> entre os blocos, exceto no último
                if (i < paragrafos.Length - 1)
                {
                    sb.Append("<br><br>");
                }
            }

            return sb.ToString();
        }


    }
}
