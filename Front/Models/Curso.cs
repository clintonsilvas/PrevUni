using System.Net.Http;
using System.Text.Json;
using Front.Models;
namespace Front.Models
{

    public class Curso
    {
        private readonly FavoritoService _favoritoService;
        public string nomeCurso { get; set; }
        public int quantAlunos { get; set; }
        public List<LogUsuario> Logs { get; set; } = new();
        public List<int> Semanas { get; set; } = new(new int[10]);
        public List<Usuario> usuarios { get; set; } = new();

        public bool IsFavorito { get; set; }


        public int engagAlto = 0;
        public int engagMedio = 0;
        public int engagBaixo = 0;

        public async Task AtualizaSemanas()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://localhost:7232/api/Moodle/logs-por-nomeCurso/{nomeCurso}");

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

        public async Task AtualizaAlunos()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://localhost:7232/api/Moodle/quantAlunos-por-nomeCurso/{nomeCurso}");

            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync();
            usuarios = JsonSerializer.Deserialize<List<Usuario>>(json) ?? [];
        }
    }


    public class CursoComAlunos
    {
        public string nomeCurso { get; set; }
        public List<Usuario> usuarios { get; set; }
    }
}
