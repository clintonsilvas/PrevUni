using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Reflection;

namespace Front.Pages;

public class PerfilAlunoModel(HttpClient httpClient) : PageModel
{
    private readonly HttpClient _httpClient = httpClient;

    [BindProperty(SupportsGet = true)]
    public string UserId { get; set; } = string.Empty;

    public Usuario User { get; set; } = new();
    public List<LogUsuario> AlunoLogs { get; set; } = [];
    public List<int> SemanasAcessoAluno { get; set; } = [];
    public Dictionary<string, int> InteracoesPorComponente { get; set; } = [];

    public List<string> ListaDeCursos { get; set; } = [];

    public float Engajamento { get; set; }

    public record AlunoEngajamento(
        [property: JsonPropertyName("userId")] string UserId,
        [property: JsonPropertyName("name")] string Nome,
        [property: JsonPropertyName("engajamento")] double Engajamento
    );

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(UserId))
            return NotFound("ID do aluno não fornecido.");

        // Faz todas as requisições paralelamente
        var logsTask = GetFromApiAsync<List<LogUsuario>>($"https://localhost:7232/api/Moodle/logs/{UserId}");
        var engajamentosTask = GetFromApiAsync<List<AlunoEngajamento>>($"https://localhost:7232/api/Mongo/engajamento-alunos");

        await Task.WhenAll(logsTask, engajamentosTask);

        // Atribui os dados recebidos
        AlunoLogs = logsTask.Result ?? [];
        var engajamentos = engajamentosTask.Result;

        ListaDeCursos = AlunoLogs.Select(l => l.course_fullname).Distinct().OrderBy(c => c).ToList();


        if (!string.IsNullOrWhiteSpace(curso))
        {
            AlunoLogs = AlunoLogs.Where(log => log.course_fullname == curso).ToList();
        }

        if (!AlunoLogs.Any())
            return NotFound("Nenhum log encontrado para o aluno.");

        // Usa o último log para montar o usuário
        var ultimoLog = AlunoLogs.Last();
        User = new Usuario
        {
            name = ultimoLog.name,
            user_id = UserId,
            user_lastaccess = ultimoLog.user_lastaccess
        };

        // Procura o engajamento do aluno
        Engajamento = (float)(engajamentos?.FirstOrDefault(e => e.UserId == UserId)?.Engajamento ?? 0);

        CalcularSemanasAcessoAluno();

        return Page();
    }


    private async Task<T?> GetFromApiAsync<T>(string url)
    {
        try
        {
            var res = await _httpClient.GetAsync(url);
            if (!res.IsSuccessStatusCode) return default;

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return default;
        }
    }


    [BindProperty(SupportsGet = true)]
    public string curso { get; set; } = string.Empty;


    private void CalcularSemanasAcessoAluno()
    {
        var datas = AlunoLogs
            .Select(log => DateTime.TryParse(log.date, out var dt) ? dt : (DateTime?)null)
            .Where(dt => dt.HasValue)
            .Select(dt => dt.Value)
            .OrderBy(d => d)
            .ToList();

        if (!datas.Any())
        {
            SemanasAcessoAluno.Clear();
            return;
        }

        var inicio = datas.First();
        SemanasAcessoAluno = datas
            .Select(d => (int)((d - inicio).TotalDays / 7))
            .GroupBy(s => s)
            .OrderBy(g => g.Key)
            .Select(g => g.Count())
            .ToList();
    }
}
