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
        private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public IaChatAlunoModel(IHttpClientFactory httpClientFactory)
            => _httpClientFactory = httpClientFactory;

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Prompt { get; set; }

        public string Resposta { get; set; }
        public string RespostaFormatada => ConverterParaHtml(Resposta);
        public string UltimaPergunta { get; set; }

        public string DadosAlunoString { get; set; } = string.Empty;
        public Usuario User { get; set; } = new();

        public List<MensagemChat> Mensagens { get; set; } = new();


        private string ChatKey => $"Chat_{UserId}";
        private string DadosAlunoKey => $"DadosAlunoString_{UserId}";

        public async Task OnGetAsync()
        {
            // --- histórico vem da sessão ---
            Mensagens = HttpContext.Session.GetObject<List<MensagemChat>>(ChatKey) ?? new();

            if (string.IsNullOrWhiteSpace(Prompt))
            {
                DadosAlunoString = await CarregarDadosAlunoAsync();
                TempData[DadosAlunoKey] = DadosAlunoString; // armazena só esse dado em TempData
                return;
            }

            // se for GET já com pergunta na query
            DadosAlunoString = TempData[DadosAlunoKey]?.ToString() ?? await CarregarDadosAlunoAsync();
            await ProcessarPergunta();

            // salva o histórico na sessão
            HttpContext.Session.SetObject(ChatKey, Mensagens);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Mensagens = HttpContext.Session.GetObject<List<MensagemChat>>(ChatKey) ?? new();

            DadosAlunoString = TempData[DadosAlunoKey]?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(DadosAlunoString))
            {
                DadosAlunoString = await CarregarDadosAlunoAsync();
                TempData[DadosAlunoKey] = DadosAlunoString;
            }

            await ProcessarPergunta();

            // persiste o histórico novamente
            HttpContext.Session.SetObject(ChatKey, Mensagens);
            return Page();
        }

        private async Task ProcessarPergunta()
        {
            if (string.IsNullOrWhiteSpace(Prompt)) return;

            var body = JsonSerializer.Serialize(new
            {
                Prompt,
                DadosAluno = DadosAlunoString,
                pos = 0
            });

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsync("https://localhost:7232/pergunte-ia",
                             new StringContent(body, Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", $"Erro ao consultar IA: {response.StatusCode}");
                return;
            }

            var result = await response.Content.ReadAsStringAsync();
            Resposta = JsonDocument.Parse(result).RootElement.GetProperty("respostaIA").GetString();
            UltimaPergunta = Prompt;

            Mensagens.Add(new MensagemChat
            {
                Pergunta = UltimaPergunta,
                Resposta = RespostaFormatada
            });

            Prompt = string.Empty;
        }

        private async Task<string> CarregarDadosAlunoAsync()
        {
            var resp = await _httpClientFactory.CreateClient()
                .GetAsync($"https://localhost:7232/api/Moodle/logs/{UserId}");
            if (!resp.IsSuccessStatusCode) return "Aluno não encontrado.";

            var raw = JsonSerializer.Deserialize<List<LogUsuario>>(
                await resp.Content.ReadAsStringAsync(), _jsonOptions);
            if (raw == null || raw.Count == 0) return "Sem registros.";

            var acoesFixas = new Acoes().ListarAcoes_paraIA();
            string NomeAcao(LogUsuario l) =>
                acoesFixas.FirstOrDefault(a =>
                    a.action == l.action &&
                    a.target == l.target &&
                    a.component == l.component
                )?.nome_acao ?? "Outros";

            var cal = CultureInfo.CurrentCulture.Calendar;

            // 1) Processa tudo e filtra “Outros”
            var logs = raw
                .Select(l =>
                {
                    var dt = DateTime.Parse(l.date);
                    var semana = cal.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    var ano = dt.Year;
                    var acao = NomeAcao(l);
                    return acao == "Outros" ? null : new
                    {
                        l.name,
                        l.user_lastaccess,
                        dt,
                        ano,
                        semana,
                        Curso = l.course_fullname,
                        Acao = acao
                    };
                })
                .Where(x => x != null)
                .ToList();

            // stats gerais
            var primeiro = logs.Min(x => x.dt);
            var ultimo = logs.Max(x => x.dt);
            var diasAtivos = logs.Select(x => x.dt.Date).Distinct().Count();
            var totInt = logs.Count;
            var infoUser = logs.OrderBy(x => x.dt).Last();

            // 2) agrega por (ano, semana) → curso → ação
            var resumo = logs
                .GroupBy(x => new { x.ano, x.semana, x.Curso, x.Acao })
                .Select(g => new {
                    g.Key.ano,
                    g.Key.semana,
                    Curso = g.Key.Curso,
                    Acao = g.Key.Acao,
                    Count = g.Count()
                })
                .GroupBy(x => new { x.ano, x.semana })
                .OrderBy(g => g.Key.ano).ThenBy(g => g.Key.semana);

            // 3) monta saída
            var sb = new StringBuilder();
            sb.Append($"Nome: {infoUser.name}/ ");
            sb.Append($"1º: {primeiro:dd/MM/yyyy}/ ");
            sb.Append($"Úl.: {ultimo:dd/MM/yyyy}/ ");
            sb.Append($"DiasAtv: {diasAtivos}/ ");
            sb.Append($"TotInt: {totInt}/ ");

            foreach (var sem in resumo)
            {
                sb.Append($"({sem.Key.ano}-W{sem.Key.semana}){{");
                foreach (var curso in sem.GroupBy(x => x.Curso).OrderBy(g => g.Key))
                {
                    var stats = curso
                        .OrderByDescending(x => x.Count)
                        .Select(x => $"{x.Acao}:{x.Count}");
                    sb.Append($"[{curso.Key} {string.Join(", ", stats)}]/");
                }
                sb.Append("}");
            }

            return sb.ToString();
        }


        public string ConverterParaHtml(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";

            texto = System.Text.RegularExpressions.Regex.Replace(
                      texto, @"\*\*(.+?)\*\*", "<strong>$1</strong>");

            var sb = new StringBuilder();
            foreach (var bloco in texto.Split("\n\n"))
            {
                var linha = bloco.Trim();
                if (linha.StartsWith("*"))
                {
                    sb.Append("<ul>");
                    foreach (var item in linha.Split('\n').Where(i => i.StartsWith("*")))
                        sb.Append($"<li>{item[1..].Trim()}</li>");
                    sb.Append("</ul>");
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(linha, @"^\d+\.")) // numerada
                {
                    sb.Append("<ol>");
                    foreach (var item in linha.Split('\n'))
                    {
                        var m = System.Text.RegularExpressions.Regex.Match(item, @"^\d+\.\s*(.+)");
                        if (m.Success) sb.Append($"<li>{m.Groups[1].Value}</li>");
                    }
                    sb.Append("</ol>");
                }
                else
                    sb.Append(linha.Replace("\n", "<br>"));

                sb.Append("<br><br>");
            }
            return sb.ToString();
        }
    }
}
