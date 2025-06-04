using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Text.Json.Serialization;

namespace Front.Pages
{
    public class PerfilAlunoModel(HttpClient httpClient) : PageModel
    {
        private readonly HttpClient _httpClient = httpClient;

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        public AlunoResumo AlunoDetalhes { get; set; }
        public List<LogUsuario> AlunoLogs { get; set; } = [];
        public List<int> SemanasAcessoAluno { get; set; } = [];
        public Dictionary<string, int> InteracoesPorComponente { get; set; } = [];

        public float Engajamento { get; set; }

        public record AlunoEngajamento(
            [property: JsonPropertyName("userId")] string UserId,
            [property: JsonPropertyName("nome")] string Nome,
            [property: JsonPropertyName("engajamento")] double Engajamento
        );

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(UserId))
                return NotFound("ID do aluno não fornecido.");

            AlunoDetalhes = await GetFromApiAsync<AlunoResumo>($"https://localhost:7232/api/Mongo/resumo-aluno/{UserId}")
                ?? new() { nome = "Aluno Não Encontrado", user_id = UserId };

            AlunoLogs = await GetFromApiAsync<List<LogUsuario>>($"https://localhost:7232/api/Moodle/logs/{UserId}") ?? [];

            var engajamento = await GetFromApiAsync<List<AlunoEngajamento>>($"https://localhost:7232/api/Mongo/engajamento-alunos");
            Engajamento = (float)(engajamento?.FirstOrDefault(e => e.UserId == UserId)?.Engajamento ?? 0);

            CalcularSemanasAcessoAluno();
            return Page();
        }

        private async Task<T?> GetFromApiAsync<T>(string url)
        {
            var res = await _httpClient.GetAsync(url);
            if (!res.IsSuccessStatusCode) return default;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private void CalcularSemanasAcessoAluno()
        {
            var datas = AlunoLogs
                .Select(l => DateTime.TryParse(l.date, out var dt) ? dt : (DateTime?)null)
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
}
