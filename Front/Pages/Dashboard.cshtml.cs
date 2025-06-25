using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace Front.Pages
{
    public class DashboardModel : PageModel
    {
        public int EngajamentoBaixo { get; set; }
        public int EngajamentoMedio { get; set; }
        public int EngajamentoAlto { get; set; }

        public Curso Cursos { get; set; } = new();
        public List<AlunoEngajamento> PossiveisDesistentes { get; set; } = new();
        public List<AlunoEngajamento> AlunosEngajamento { get; set; } = new();

        public class AlunoEngajamento
        {
            [JsonPropertyName("userId")]
            public string UserId { get; set; } = "";

            [JsonPropertyName("nome")]
            public string Name { get; set; } = "";

            [JsonPropertyName("engajamento")]
            public double Engajamento { get; set; }
        }

        public class AcaoResumo
        {
            public string Acao { get; set; } = "";
            public int Quantidade { get; set; }
        }

        public string LblJson { get; set; } = string.Empty;
        public string DsJson { get; set; } = string.Empty;
        public string AnosJson { get; set; } = string.Empty;
        public string SemanasJson { get; set; } = string.Empty;
        public string Labels { get; set; } = string.Empty;
        public string Dados { get; set; } = string.Empty;
        public List<AcaoResumo> AcoesResumo { get; set; } = new();

        public async Task OnGetAsync(string curso)
        {
            Cursos = new Curso { nomeCurso = curso };
        }

        public async Task<PartialViewResult> OnGetCarregamentoAsync(string curso)
        {
            Cursos = new Curso { nomeCurso = curso };
            using var http = new HttpClient();

            await CarregarAlunosAsync(http, curso);
            await Cursos.AtualizaSemanas();
            CalcularSemanas();

            await CarregarEngajamentoAsync(http, curso); // <-- passe o curso aqui
            CalcularEngajamento();
            CalcularDesistentes();

            return Partial("Cursos/_Graficos_Curso", this);
        }

        private async Task CarregarAlunosAsync(HttpClient httpClient, string curso)
        {
            var resp = await httpClient.GetAsync($"https://localhost:7232/api/Moodle/alunos-por-curso/{curso}");
            if (!resp.IsSuccessStatusCode)
            {
                Cursos.usuarios = new List<Usuario>();
                Cursos.quantAlunos = 0;
                return;
            }

            var json = await resp.Content.ReadAsStringAsync();
            Cursos.usuarios = JsonSerializer.Deserialize<List<Usuario>>(json) ?? new();
        }

        private async Task CarregarEngajamentoAsync(HttpClient httpClient, string curso)
        {
            var resp = await httpClient.GetAsync($"https://localhost:7232/api/Engajamento/curso/{curso}");
            if (!resp.IsSuccessStatusCode)
            {
                AlunosEngajamento = new();
                return;
            }

            var json = await resp.Content.ReadAsStringAsync();
            var todos = JsonSerializer.Deserialize<List<AlunoEngajamento>>(json) ?? new();

            var idsAlunos = Cursos.usuarios.Select(u => u.user_id).ToHashSet();
            AlunosEngajamento = todos.Where(a => idsAlunos.Contains(a.UserId)).ToList();
        }


        private void CalcularEngajamento()
        {
            var total = AlunosEngajamento.Count;
            if (total == 0)
            {
                EngajamentoAlto = EngajamentoMedio = EngajamentoBaixo = 0;
                return;
            }

            EngajamentoAlto = AlunosEngajamento.Count(a => a.Engajamento > 66) * 100 / total;
            EngajamentoMedio = AlunosEngajamento.Count(a => a.Engajamento >= 33 && a.Engajamento <= 66) * 100 / total;
            EngajamentoBaixo = 100 - (EngajamentoAlto + EngajamentoMedio);
        }

        private void CalcularDesistentes()
        {
            PossiveisDesistentes = AlunosEngajamento
                .Where(a => a.Engajamento < 30.0)
                .ToList();
        }

        private void CalcularSemanas()
        {
            var acoesFixas = new Acoes().ListarAcoes();
            string AcaoDe(string a, string t, string c) => acoesFixas.FirstOrDefault(x => x.action == a && x.target == t && x.component == c)?.nome_acao ?? "Outros";

            var cal = CultureInfo.CurrentCulture.Calendar;

            var logsProcessados = Cursos.Logs.Select(l => new
            {
                Semana = cal.GetWeekOfYear(DateTime.Parse(l.date), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),
                Ano = DateTime.Parse(l.date).Year,
                Nome = AcaoDe(l.action, l.target, l.component)
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
}
