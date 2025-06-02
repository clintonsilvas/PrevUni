using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Text.Json;

namespace Front.Pages
{
    public class DashboardModel : PageModel
    {
        public Curso Cursos { get; set; } = new();
        public List<AlunoResumo> Alunos { get; set; } = new();

        public List<AlunoDesistente> PossiveisDesistentes { get; set; } = new();

        public class AlunoDesistente
        {
            public string UserId { get; set; }
            public string Nome { get; set; }
            public int QuantidadeLogs { get; set; }
        }

        public async Task OnGetAsync(string curso)
        {
            Cursos = new Curso()
            {
                curso = curso
            };

            using var httpClient = new HttpClient();

            var responseAlunos = await httpClient.GetAsync($"https://localhost:7232/api/Moodle/alunos-por-curso/{curso}");
            if (responseAlunos.IsSuccessStatusCode)
            {
                var jsonAlunos = await responseAlunos.Content.ReadAsStringAsync();
                Alunos = JsonSerializer.Deserialize<List<AlunoResumo>>(jsonAlunos) ?? new();
                Cursos.alunos = Alunos.Count;
            }
            else
            {
                Alunos = new List<AlunoResumo>();
                Cursos.alunos = 0;
            }

            // Aguarda os dados de semanas (suponho que também carrega logs)
            await Cursos.AtualizaSemanas();

            // Agora calcula possíveis desistentes pela média de logs:
            var contadorLogs = new Dictionary<string, int>();
            var nomes = new Dictionary<string, string>();

            // Percorre os logs do curso
            foreach (var log in Cursos.Logs)
            {
                if (!contadorLogs.ContainsKey(log.user_id))
                {
                    contadorLogs[log.user_id] = 0;
                    nomes[log.user_id] = log.name; // ou log.nome, conforme sua propriedade
                }
                contadorLogs[log.user_id]++;
            }

            // Calcula média dos logs por aluno
            double mediaLogs = 0;
            if (contadorLogs.Count > 0)
                mediaLogs = contadorLogs.Values.Average();

            // Filtra os alunos com menos logs que a média
            PossiveisDesistentes = contadorLogs
                .Where(x => x.Value < mediaLogs)
                .Select(x => new AlunoDesistente
                {
                    UserId = x.Key,
                    Nome = nomes[x.Key],
                    QuantidadeLogs = x.Value
                })
                .ToList();
        }
    }
}
