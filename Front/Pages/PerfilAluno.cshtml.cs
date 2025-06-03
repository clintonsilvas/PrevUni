using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Collections.Generic; 
using System.Globalization;
using System;

namespace Front.Pages
{
    public static class DateTimeExtensions
    {
        public static int WeekOfYear(this DateTime date)
        {
            var culture = System.Globalization.CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }
    }
    public class PerfilAlunoModel : PageModel
    {
        private readonly HttpClient _httpClient;

        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; }

        public AlunoResumo AlunoDetalhes { get; set; }
        public List<LogUsuario> AlunoLogs { get; set; }
        public List<int> SemanasAcessoAluno { get; set; } = new List<int>(new int[10]);
        public Dictionary<string, int> InteracoesPorComponente { get; set; } = new Dictionary<string, int>();

        public double EngajamentoAlto { get; set; }
        public double EngajamentoMedio { get; set; }
        public double EngajamentoBaixo { get; set; }

        public PerfilAlunoModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return NotFound("ID do aluno não fornecido.");
            }

            var responseResumo = await _httpClient.GetAsync($"https://localhost:7232/resumo-aluno/{UserId}");
            if (responseResumo.IsSuccessStatusCode)
            {
                var jsonResumo = await responseResumo.Content.ReadAsStringAsync();
                AlunoDetalhes = JsonSerializer.Deserialize<AlunoResumo>(jsonResumo, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                AlunoDetalhes = new AlunoResumo { nome = "Aluno Não Encontrado", user_id = UserId };
                return Page();
            }

            var responseLogs = await _httpClient.GetAsync($"https://localhost:7232/api/Moodle/logs/{UserId}");
            if (responseLogs.IsSuccessStatusCode)
            {
                var jsonLogs = await responseLogs.Content.ReadAsStringAsync();
                AlunoLogs = JsonSerializer.Deserialize<List<LogUsuario>>(jsonLogs, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                AlunoLogs = new List<LogUsuario>();
            }

            CalcularEngajamento();

            CalcularSemanasAcessoAluno();

           // CalcularInteracoesPorComponente();

            return Page();
        }

        private void CalcularEngajamento()
        {
            if (AlunoDetalhes == null) return;

            // Lógica parcial
            if (AlunoDetalhes.total_acessos > 1000)
            {
                EngajamentoAlto = 100;
                EngajamentoMedio = 0;
                EngajamentoBaixo = 0;
            }
            else if (AlunoDetalhes.total_acessos > 500)
            {
                EngajamentoAlto = 0;
                EngajamentoMedio = 100;
                EngajamentoBaixo = 0;
            }
            else
            {
                EngajamentoAlto = 0;
                EngajamentoMedio = 0;
                EngajamentoBaixo = 100;
            }
        }



        private void CalcularSemanasAcessoAluno()
        {
            if (AlunoLogs == null || !AlunoLogs.Any())
            {
                SemanasAcessoAluno = new List<int>();
                return;
            }

            DateTime primeiroAcesso = AlunoLogs
                .Where(l => DateTime.TryParse(l.date, out _))
                .Select(l => DateTime.Parse(l.date))
                .DefaultIfEmpty(DateTime.Now)
                .Min();

            SemanasAcessoAluno = new List<int>();

            foreach (var log in AlunoLogs)
            {
                if (DateTime.TryParse(log.date, out DateTime dataLog))
                {
                    TimeSpan diferenca = dataLog - primeiroAcesso;
                    int semana = (int)(diferenca.TotalDays / 7) + 1;

                    while (SemanasAcessoAluno.Count < semana)
                    {
                        SemanasAcessoAluno.Add(0);
                    }
                    SemanasAcessoAluno[semana - 1]++;
                }
            }
        }
        //private void CalcularInteracoesPorComponente()
        //{
        //    InteracoesPorComponente = new Dictionary<string, int>();
        //    if (AlunoDetalhes?.interacoes_por_componente != null)
        //    {
        //        InteracoesPorComponente = AlunoDetalhes.interacoes_por_componente;
        //    }
        //    else if (AlunoLogs != null && AlunoLogs.Any())
        //    {
        //        foreach (var log in AlunoLogs)
        //        {
        //            if (!string.IsNullOrEmpty(log.component))
        //            {
        //                if (InteracoesPorComponente.ContainsKey(log.component))
        //                {
        //                    InteracoesPorComponente[log.component]++;
        //                }
        //                else
        //                {
        //                    InteracoesPorComponente.Add(log.component, 1);
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
