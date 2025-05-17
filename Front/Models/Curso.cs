using System.Net.Http;
using System.Text.Json;
using Front.Models;

namespace Front.Models
{
    public class Curso
    {
        public string curso { get; set; }
        public int alunos { get; set; }

        public List<LogUsuario> Logs { get; set; } = new();

        // Lista para armazenar as contagens de cada semana, iniciada com 10 semanas zeradas
        public List<int> Semanas { get; set; } = new List<int>(new int[10]);

        public async Task AtualizaSemanas()
        {
            using var httpClient = new HttpClient();
            var responseLogs = await httpClient.GetAsync($"https://localhost:7232/api/Moodle/logs-por-curso/{this.curso}");
            if (responseLogs.IsSuccessStatusCode)
            {
                var json = await responseLogs.Content.ReadAsStringAsync();
                Logs = JsonSerializer.Deserialize<List<LogUsuario>>(json) ?? new();
            }

            if (Logs == null || !Logs.Any())
                return;

            DateTime PrimeiroAcesso = Logs.Min(l => DateTime.Parse(l.date));

            // Resetar semanas para garantir contagem correta a cada atualização
            for (int i = 0; i < Semanas.Count; i++)
                Semanas[i] = 0;

            foreach (LogUsuario l in Logs)
            {
                var dataLog = DateTime.Parse(l.date);
                var diasDesdePrimeiroAcesso = (dataLog - PrimeiroAcesso).TotalDays;
                int semana = (int)(diasDesdePrimeiroAcesso / 7) + 1;

                int semanaIndex = semana - 1;

                // Aumenta a lista se precisar de mais semanas
                if (semanaIndex >= Semanas.Count)
                {
                    while (Semanas.Count <= semanaIndex)
                        Semanas.Add(0);
                }

                Semanas[semanaIndex]++;
            }
        }
    }
}
