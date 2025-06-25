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
    [BindProperty(SupportsGet = true)] public string nome { get; set; } = string.Empty;


    public Usuario User { get; private set; } = new();
    public List<LogUsuario> AlunoLogs { get; private set; } = new List<LogUsuario>();
    public List<string> ListaDeCursos { get; private set; } = new List<string>();
    public List<int> SemanasAcessoAluno { get; private set; } = new List<int>();
    public Dictionary<string, int> InteracoesPorComponente { get; private set; } = new Dictionary<string, int>();
    public List<AcaoResumo> AcoesResumo { get; private set; } = new List<AcaoResumo>();
    private string CursoSelecionado { get; set; } = string.Empty;
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

    public async Task<IActionResult> OnGetCarregamentoAsync(string? curso = null)
    {
        if (string.IsNullOrWhiteSpace(UserId))
            return NotFound("ID do aluno não fornecido.");

        if (!string.IsNullOrWhiteSpace(curso))
            CursoSelecionado = curso.Trim();

        var resultado = await ObterDadosAlunoAsync(UserId);
        Console.WriteLine($"Partial {UserId},{curso}");

        return Partial("Alunos/GraficoPerfilAluno", this);
    }

    private async Task<bool> ObterDadosAlunoAsync(string userId)
    {
        var logsTask = GetFromApiAsync<List<LogUsuario>>($"https://localhost:7232/api/Moodle/logs/{userId}");
        var engajamentosTask = GetFromApiAsync<List<AlunoEngajamento>>($"https://localhost:7232/api/engajamento/curso/{Uri.EscapeDataString(CursoSelecionado)}");


        await Task.WhenAll(logsTask, engajamentosTask);
        List<LogUsuario> AlunoLogsAux = new List<LogUsuario>();

        AlunoLogsAux = logsTask.Result ?? new List<LogUsuario>();
        var engajamentos = engajamentosTask.Result;

        if (!AlunoLogsAux.Any()) return false;

        ListaDeCursos = AlunoLogsAux.Select(l => l.course_fullname).Distinct().OrderBy(c => c).ToList();

        if (!string.IsNullOrWhiteSpace(CursoSelecionado))
        {
            AlunoLogs = AlunoLogsAux
                .Where(log => string.Equals(log.course_fullname?.Trim(), CursoSelecionado.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        else
        {
            AlunoLogs = AlunoLogsAux;
        }

        if (!AlunoLogs.Any()) return false;
        CalcularSemanas();

        User = CriarUsuario(AlunoLogs[^1], userId);
        Engajamento = ObterEngajamento(engajamentos, userId);

        return true;
    }

    private Usuario CriarUsuario(LogUsuario ultimoLog, string userId) => new()
    {
        name = ultimoLog.name,
        user_id = userId,
        user_lastaccess = ultimoLog.user_lastaccess
    };
   
    private float ObterEngajamento(List<AlunoEngajamento>? engajamentos, string userId) =>  // OLHAR AQUI DPS 
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
        var acoesFixas = new Acoes().ListarAcoes();
        string AcaoDe(string a, string t, string c) => acoesFixas.FirstOrDefault(x => x.action == a && x.target == t && x.component == c)?.nome_acao ?? "Outros";

        var cal = CultureInfo.CurrentCulture.Calendar;

        var logsProcessados = AlunoLogs.Select(l =>
        {
            var data = DateTime.Parse(l.date);
            return new
            {
                Semana = cal.GetWeekOfYear(data, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),
                Ano = data.Year,
                Nome = AcaoDe(l.action, l.target, l.component)
            };
        }).ToList();

        var grupos = logsProcessados.GroupBy(x => new { x.Ano, x.Semana, x.Nome })
            .Select(g => new { g.Key.Ano, g.Key.Semana, Acao = g.Key.Nome, Qtde = g.Count() })
            .OrderBy(g => g.Ano).ThenBy(g => g.Semana).ToList();

        var semanasOrdenadas = grupos.Select(g => new { g.Ano, g.Semana }).Distinct().OrderBy(s => s.Ano).ThenBy(s => s.Semana).ToList();

        var labels = semanasOrdenadas.Select((s, i) => (i + 1).ToString()).ToList();
        var nomesAcoes = grupos.Select(g => g.Acao).Distinct().ToList();

        var datasets = nomesAcoes.Select((nome, idx) => new
        {
            label = nome,
            data = semanasOrdenadas.Select(s =>
                grupos.FirstOrDefault(g => g.Ano == s.Ano && g.Semana == s.Semana && g.Acao == nome)?.Qtde ?? 0).ToList(),
            backgroundColor = $"hsl({idx * 60},70%,60%)",
            stack = "stack1",
            borderRadius = 10,
            barPercentage = 0.5,
            categoryPercentage = 0.8
        }).ToList();

        LblJson = JsonSerializer.Serialize(labels);
        DsJson = JsonSerializer.Serialize(datasets);
        AnosJson = JsonSerializer.Serialize(semanasOrdenadas.Select(s => s.Ano));
        SemanasJson = JsonSerializer.Serialize(semanasOrdenadas.Select(s => s.Semana));

        AcoesResumo = logsProcessados.GroupBy(x => x.Nome)
            .Select(g => new AcaoResumo { Acao = g.Key, Quantidade = g.Count() })
            .ToList();

        Labels = string.Join(",", AcoesResumo.Select(a => $"'{a.Acao}'"));
        Dados = string.Join(",", AcoesResumo.Select(a => a.Quantidade));
    }
}