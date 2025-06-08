using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Front.Pages
{
    public class DashboardModel : PageModel
    {




        public Curso Cursos { get; set; } = new();
        public List<AlunoResumo> Alunos { get; set; } = new();

        public List<AlunoDesistente> PossiveisDesistentes { get; set; } = new();
        public List<AlunoEngajamento> AlunosEngajamento { get; set; } = new();


        public class AlunoDesistente
        {
            public string UserId { get; set; }
            public string Nome { get; set; }

            public double Engajamento { get; set; }
            public int QuantidadeLogs { get; set; }
        }

        public class AlunoEngajamento
        {
            [JsonPropertyName("userId")]
            public string UserId { get; set; }

            [JsonPropertyName("nome")]
            public string Nome { get; set; }

            [JsonPropertyName("engajamento")]
            public double Engajamento { get; set; }
        }

        public async Task OnGetAsync(string curso)
        {
            Cursos = new Curso()
            {
                curso = curso
            };

            using var httpClient = new HttpClient();

            // Buscar alunos do curso
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

            await Cursos.AtualizaSemanas();

            // Buscar dados de engajamento
            var responseEngajamento = await httpClient.GetAsync("https://localhost:7232/api/Mongo/engajamento-alunos");
            if (responseEngajamento.IsSuccessStatusCode)
            {
                var jsonEngajamento = await responseEngajamento.Content.ReadAsStringAsync();
                var todosAlunosEngajamento = JsonSerializer.Deserialize<List<AlunoEngajamento>>(jsonEngajamento) ?? new();

                AlunosEngajamento = todosAlunosEngajamento
                .Where(a => Alunos.Any(al => al.user_id == a.UserId))
                .ToList();



            }
            else
            {
                AlunosEngajamento = new List<AlunoEngajamento>();
            }

            //  limiar de engajamento para considerar como desistente (30%)
            double limiarEngajamento = 30.0;

            // Filtra alunos com engajamento abaixo do limiar
            PossiveisDesistentes = AlunosEngajamento
                .Where(a => a.Engajamento < limiarEngajamento)
                .Select(a => new AlunoDesistente
                {
                    UserId = a.UserId,
                    Nome = a.Nome,
                    Engajamento = a.Engajamento
                })
                .ToList();
        }

    }
}
