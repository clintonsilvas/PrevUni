using System.Net.Http;
using System.Text.Json;

namespace Front.Models
{
    public class Curso
    {
        public string curso { get; set; }
        public int alunos { get; set; }
        public List<LogUsuario> Logs { get; set; } = new();
        public List<int> Semanas { get; set; } = new(new int[10]);

        public async Task AtualizaSemanas()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://localhost:7232/api/Moodle/logs-por-curso/{curso}");

            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync();
            Logs = JsonSerializer.Deserialize<List<LogUsuario>>(json) ?? [];

            if (Logs.Count == 0) return;

            var primeiroAcesso = Logs.Min(l => DateTime.Parse(l.date));
            Semanas = new List<int>(new int[10]); // Resetar

            foreach (var log in Logs)
            {
                var data = DateTime.Parse(log.date);
                var dias = (data - primeiroAcesso).TotalDays;
                var semanaIndex = (int)(dias / 7);

                if (semanaIndex >= Semanas.Count)
                    Semanas.AddRange(Enumerable.Repeat(0, semanaIndex - Semanas.Count + 1));

                Semanas[semanaIndex]++;
            }
        }
    }
}
