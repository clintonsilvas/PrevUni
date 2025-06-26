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
    [BindProperty(SupportsGet = true)] public int mes { get; set; }
    [BindProperty(SupportsGet = true)] public int ano { get; set; }

    public Usuario User { get; private set; } = new();
    public List<LogUsuario> AlunoLogs { get; private set; } = [];
    public List<string> ListaDeCursos { get; private set; } = [];
    public List<AcaoResumo> AcoesResumo { get; private set; } = [];
    public float Engajamento { get; private set; }

    public string LblJson { get; private set; } = string.Empty;
    public string DsJson { get; private set; } = string.Empty;
    public string AnosJson { get; private set; } = string.Empty;
    public string SemanasJson { get; private set; } = string.Empty;
    public string Labels { get; private set; } = string.Empty;
    public string Dados { get; private set; } = string.Empty;

    public CalendarioInfos Calendario { get; set; } = new();

    public record AlunoEngajamento(
        [property: JsonPropertyName("userId")] string UserId,
        [property: JsonPropertyName("name")] string Nome,
        [property: JsonPropertyName("engajamento")] double Engajamento);

    public class AcaoResumo { public string Acao { get; set; } = string.Empty; public int Quantidade { get; set; } }

    public async Task<IActionResult> OnGetCarregamentoAsync(string? curso = null)
    {
        if (string.IsNullOrWhiteSpace(UserId)) return NotFound("ID do aluno não fornecido.");

        if (!string.IsNullOrWhiteSpace(curso)) this.curso = curso.Trim();

        var carregado = await CarregarDadosAlunoAsync();
        return Partial("Alunos/GraficoPerfilAluno", this);
    }

    public async Task<IActionResult> OnGetCalendarioParcialAsync()
    {
        var logs = await GetFromApiAsync<List<LogUsuario>>($"https://localhost:7232/api/Unifenas/logs/{UserId}");

        if (logs == null || logs.Count == 0)
            return Partial("Alunos/Calendario", new CalendarioInfos()); // retorna vazio se nada encontrado

        List<LogUsuario> logsFiltrados;

        if (!string.IsNullOrWhiteSpace(curso))
        {
            logsFiltrados = logs
                .Where(log => string.Equals(log.course_fullname?.Trim(), curso.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        else
        {
            logsFiltrados = logs;
        }

        var calendario = new CalendarioInfos();
        calendario.SetCalendario(mes, ano, logsFiltrados);

        return Partial("Alunos/Calendario", calendario);
    }

    private async Task<bool> CarregarDadosAlunoAsync()
    {
        var logsUrl = $"https://localhost:7232/api/Unifenas/logs/{UserId}";
        var logsTask = GetFromApiAsync<List<LogUsuario>>(logsUrl);

        await Task.WhenAll(logsTask);

        var logs = logsTask.Result ?? [];
        if (logs.Count == 0) return false;

        ListaDeCursos = logs.Select(l => l.course_fullname).Distinct().OrderBy(c => c).ToList();

        var cursoSelecionado = curso?.Trim();
        AlunoLogs = string.IsNullOrWhiteSpace(cursoSelecionado)
            ? logs
            : logs.Where(l => string.Equals(l.course_fullname?.Trim(), cursoSelecionado, StringComparison.OrdinalIgnoreCase)).ToList();

        if (AlunoLogs.Count == 0) return false;

        List<AlunoEngajamento> engajamentos = [];

        if (string.IsNullOrWhiteSpace(cursoSelecionado))
        {
            var engajamentoTasks = ListaDeCursos
                .Select(c => GetFromApiAsync<List<AlunoEngajamento>>($"https://localhost:7232/api/engajamento/curso/{Uri.EscapeDataString(c)}"))
                .ToList();

            var engajamentoResults = await Task.WhenAll(engajamentoTasks);

            engajamentos = engajamentoResults
                .Where(r => r != null)
                .SelectMany(r => r!)
                .Where(e => e.UserId == UserId)
                .ToList();

            Engajamento = engajamentos.Count > 0
                ? (float)engajamentos.Average(e => e.Engajamento)
                : 0;
        }
        else
        {
            var engUrl = $"https://localhost:7232/api/engajamento/curso/{Uri.EscapeDataString(cursoSelecionado)}";
            var engajamentosResult = await GetFromApiAsync<List<AlunoEngajamento>>(engUrl);
            Engajamento = (float)(engajamentosResult?.FirstOrDefault(e => e.UserId == UserId)?.Engajamento ?? 0);
        }

        CalcularSemanas();
        User = CriarUsuario(AlunoLogs[^1]);
        Calendario.SetCalendario(mes, ano, AlunoLogs);

        return true;
    }

    private Usuario CriarUsuario(LogUsuario ultimoLog) => new()
    {
        name = ultimoLog.name,
        user_id = UserId,
        user_lastaccess = ultimoLog.user_lastaccess
    };

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

    private void CalcularSemanas()
    {
        var acoesFixas = new Acoes().ListarAcoes();
        string AcaoDe(string a, string t, string c) => acoesFixas.FirstOrDefault(x => x.action == a && x.target == t && x.component == c)?.nome_acao ?? "Outros";

        var cal = CultureInfo.CurrentCulture.Calendar;

        var logsProcessados = AlunoLogs.Select(l => new
        {
            Data = DateTime.TryParse(l.date, out var data) ? data : (DateTime?)null,
            Nome = AcaoDe(l.action, l.target, l.component)
        })
        .Where(x => x.Data != null)
        .Select(x => new
        {
            Semana = cal.GetWeekOfYear(x.Data!.Value, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),
            Ano = x.Data.Value.Year,
            x.Nome
        })
        .ToList();

        var grupos = logsProcessados.GroupBy(x => new { x.Ano, x.Semana, x.Nome })
            .Select(g => new { g.Key.Ano, g.Key.Semana, Acao = g.Key.Nome, Qtde = g.Count() })
            .OrderBy(g => g.Ano).ThenBy(g => g.Semana).ToList();

        var semanasOrdenadas = grupos.Select(g => new { g.Ano, g.Semana }).Distinct().OrderBy(s => s.Ano).ThenBy(s => s.Semana).ToList();
        var labels = semanasOrdenadas.Select((s, i) => (i + 1).ToString()).ToList();
        var nomesAcoes = grupos.Select(g => g.Acao).Distinct().ToList();

        var datasets = nomesAcoes.Select((nome, idx) => new
        {
            label = nome,
            data = semanasOrdenadas.Select(s => grupos.FirstOrDefault(g => g.Ano == s.Ano && g.Semana == s.Semana && g.Acao == nome)?.Qtde ?? 0).ToList(),
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

public class CalendarioInfos
{
    public int mes { get; set; }
    public int ano { get; set; }
    public List<LogUsuario> logs { get; set; } = new List<LogUsuario>();


    public void SetCalendario(int mesRecebido, int anoRecebido, List<LogUsuario> todosLogs)
    {
        var hoje = DateTime.Now;

        mes = (mesRecebido >= 1 && mesRecebido <= 12) ? mesRecebido : hoje.Month;
        ano = (anoRecebido > 0) ? anoRecebido : hoje.Year;

        logs = todosLogs
            .Where(log =>
            {
                if (DateTime.TryParse(log.date, out var data))
                    return data.Month == mes && data.Year == ano;
                return false;
            })
            .ToList();
    }

    public string AcaoDe(string a, string t, string c) =>
    new Acoes().ListarAcoes()
        .FirstOrDefault(x => x.action == a && x.target == t && x.component == c)?.nome_acao ?? "Outros";

}