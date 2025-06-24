using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;

namespace Front.Pages;

public class PerfilAlunoModel(HttpClient httpClient) : PageModel
{
    private readonly HttpClient _httpClient = httpClient;

    [BindProperty(SupportsGet = true)] public string UserId { get; set; } = string.Empty;
    [BindProperty(SupportsGet = true)] public string curso { get; set; } = string.Empty;

    public Usuario User { get; private set; } = new();
    public List<LogUsuario> AlunoLogs { get; private set; } = [];
    public List<string> ListaDeCursos { get; private set; } = [];
    public List<int> SemanasAcessoAluno { get; private set; } = [];
    public Dictionary<string, int> InteracoesPorComponente { get; private set; } = [];
    public List<AcaoResumo> AcoesResumo { get; private set; } = [];
    public float Engajamento { get; private set; }

    public string LblJson { get; private set; } = string.Empty;
    public string DsJson { get; private set; } = string.Empty;
    public string AnosJson { get; private set; } = string.Empty;
    public string SemanasJson { get; private set; } = string.Empty;
    public string Labels { get; private set; } = string.Empty;
    public string Dados { get; private set; } = string.Empty;

    public record AlunoEngajamento(
        [property: JsonPropertyName("userId")] string UserId,
        [property: JsonPropertyName("name")] string Nome,
        [property: JsonPropertyName("engajamento")] double Engajamento);

    public class AcaoResumo
    {
        public string Acao { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(UserId))
            return NotFound("ID do aluno não fornecido.");

        var resultado = await ObterDadosAlunoAsync(UserId);
        if (!resultado)
            return NotFound("Nenhum log encontrado para o aluno.");

        return Page();
    }

    private async Task<bool> ObterDadosAlunoAsync(string userId)
    {
        var logsTask = GetFromApiAsync<List<LogUsuario>>($"https://localhost:7232/api/Moodle/logs/{userId}");
        var engajamentosTask = GetFromApiAsync<List<AlunoEngajamento>>("https://localhost:7232/api/Mongo/engajamento-alunos");

        await Task.WhenAll(logsTask, engajamentosTask);

        AlunoLogs = logsTask.Result ?? [];
        var engajamentos = engajamentosTask.Result;

        if (!AlunoLogs.Any()) return false;

        ListaDeCursos = AlunoLogs.Select(l => l.course_fullname).Distinct().OrderBy(c => c).ToList();
        if (!string.IsNullOrWhiteSpace(curso))
            AlunoLogs = AlunoLogs.Where(l => l.course_fullname == curso).ToList();

        if (!AlunoLogs.Any()) return false;

        User = CriarUsuario(AlunoLogs[^1], userId);
        Engajamento = ObterEngajamento(engajamentos, userId);

        CalcularSemanas();
        return true;
    }

    private Usuario CriarUsuario(LogUsuario ultimoLog, string userId) => new()
    {
        name = ultimoLog.name,
        user_id = userId,
        user_lastaccess = ultimoLog.user_lastaccess
    };

    private float ObterEngajamento(List<AlunoEngajamento>? engajamentos, string userId) =>
        (float)(engajamentos?.FirstOrDefault(e => e.UserId == userId)?.Engajamento ?? 0);

    private async Task<T?> GetFromApiAsync<T>(string url)
    {
        try
        {
            var res = await _httpClient.GetAsync(url);
            if (!res.IsSuccessStatusCode) return default;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch { return default; }
    }

    public string AcaoDe(string a, string t, string c) =>
        new Acoes().ListarAcoes()
            .FirstOrDefault(x => x.action == a && x.target == t && x.component == c)?.nome_acao ?? "Outros";

    private void CalcularSemanas()
    {
        var cal = CultureInfo.CurrentCulture.Calendar;
        var logsProcessados = AlunoLogs.Select(l => new
        {
            Semana = cal.GetWeekOfYear(DateTime.Parse(l.date), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),
            Ano = DateTime.Parse(l.date).Year,
            Nome = AcaoDe(l.action, l.target, l.component)
        }).ToList();

        var grupos = logsProcessados
            .GroupBy(x => new { x.Ano, x.Semana, x.Nome })
            .Select(g => new { g.Key.Ano, g.Key.Semana, Acao = g.Key.Nome, Qtde = g.Count() })
            .OrderBy(g => g.Ano).ThenBy(g => g.Semana).ToList();

        var semanas = grupos.Select(g => new { g.Ano, g.Semana }).Distinct().OrderBy(s => s.Ano).ThenBy(s => s.Semana).ToList();
        var labels = semanas.Select((_, i) => (i + 1).ToString()).ToList();
        var nomesAcoes = grupos.Select(g => g.Acao).Distinct().ToList();

        var datasets = nomesAcoes.Select((nome, idx) => new
        {
            label = nome,
            data = semanas.Select(s => grupos.FirstOrDefault(g => g.Ano == s.Ano && g.Semana == s.Semana && g.Acao == nome)?.Qtde ?? 0).ToList(),
            backgroundColor = $"hsl({idx * 60},70%,60%)",
            stack = "stack1",
            borderRadius = 10,
            barPercentage = 0.5,
            categoryPercentage = 0.8
        }).ToList();

        LblJson = JsonSerializer.Serialize(labels);
        DsJson = JsonSerializer.Serialize(datasets);
        AnosJson = JsonSerializer.Serialize(semanas.Select(s => s.Ano));
        SemanasJson = JsonSerializer.Serialize(semanas.Select(s => s.Semana));

        AcoesResumo = logsProcessados
            .GroupBy(x => x.Nome)
            .Select(g => new AcaoResumo { Acao = g.Key, Quantidade = g.Count() })
            .ToList();

        Labels = string.Join(",", AcoesResumo.Select(a => $"'{a.Acao}'"));
        Dados = string.Join(",", AcoesResumo.Select(a => a.Quantidade));
    }
}