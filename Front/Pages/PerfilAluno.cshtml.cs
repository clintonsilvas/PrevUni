using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Globalization;
using Front.Models;

namespace Front.Pages
{
    public class PerfilAlunoModel : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        public AlunoResumo AlunoDetalhes { get; set; }
        public List<LogUsuario> AlunoLogs { get; set; } = new();
        public List<int> SemanasAcessoAluno { get; set; } = new();
        public Dictionary<string, int> InteracoesPorComponente { get; set; } = new();

        public double EngajamentoAlto { get; set; }
        public double EngajamentoMedio { get; set; }
        public double EngajamentoBaixo { get; set; }

        public PerfilAlunoModel(HttpClient httpClient) => _httpClient = httpClient;

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(UserId))
                return NotFound("ID do aluno não fornecido.");

            AlunoDetalhes = await GetFromApiAsync<AlunoResumo>($"https://localhost:7232/api/Mongo/resumo-aluno/{UserId}");
            if (AlunoDetalhes == null)
            {
                AlunoDetalhes = new AlunoResumo { nome = "Aluno Não Encontrado", user_id = UserId };
                return Page();
            }

            AlunoLogs = await GetFromApiAsync<List<LogUsuario>>($"https://localhost:7232/api/Moodle/logs/{UserId}") ?? new List<LogUsuario>();

            CalcularEngajamento();
            CalcularSemanasAcessoAluno();
            CalcularInteracoesPorComponente();

            return Page();
        }

        private async Task<T> GetFromApiAsync<T>(string url)
        {
            var res = await _httpClient.GetAsync(url);
            if (!res.IsSuccessStatusCode) return default;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private void CalcularEngajamento()
        {
            var acessos = AlunoDetalhes?.total_acessos ?? 0;
            EngajamentoAlto = acessos > 1000 ? 100 : 0;
            EngajamentoMedio = acessos is > 500 and <= 1000 ? 100 : 0;
            EngajamentoBaixo = acessos <= 500 ? 100 : 0;
        }

        private void CalcularSemanasAcessoAluno()
        {
            if (AlunoLogs == null || AlunoLogs.Count == 0)
            {
                SemanasAcessoAluno.Clear();
                return;
            }

            var datasValidas = AlunoLogs
                .Select(l => DateTime.TryParse(l.date, out var dt) ? dt : (DateTime?)null)
                .Where(dt => dt.HasValue)
                .Select(dt => dt.Value)
                .OrderBy(d => d)
                .ToList();

            if (datasValidas.Count == 0)
            {
                SemanasAcessoAluno.Clear();
                return;
            }

            var primeiroAcesso = datasValidas.First();
            SemanasAcessoAluno = new List<int>();

            foreach (var data in datasValidas)
            {
                var semana = (int)((data - primeiroAcesso).TotalDays / 7);
                while (SemanasAcessoAluno.Count <= semana)
                    SemanasAcessoAluno.Add(0);

                SemanasAcessoAluno[semana]++;
            }
        }

        private void CalcularInteracoesPorComponente()
        {
            if (AlunoDetalhes?.interacoes_por_componente != null)
            {
                InteracoesPorComponente = new Dictionary<string, int>(AlunoDetalhes.interacoes_por_componente);
                return;
            }

            InteracoesPorComponente = AlunoLogs?
                .Where(l => !string.IsNullOrEmpty(l.component))
                .GroupBy(l => l.component)
                .ToDictionary(g => g.Key, g => g.Count())
                ?? new Dictionary<string, int>();
        }


    }
}
