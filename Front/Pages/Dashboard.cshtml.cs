using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace Front.Pages
{
    public class DashboardModel : PageModel
    {
        public int EngajamentoBaixo { get; set; }
        public int EngajamentoMedio { get; set; }
        public int EngajamentoAlto { get; set; }




        public Curso Cursos { get; set; } = new();
        //public List<AlunoResumo> Alunos { get; set; } = new();

        public List<AlunoDesistente> PossiveisDesistentes { get; set; } = new();
        public List<AlunoEngajamento> AlunosEngajamento { get; set; } = new();


        public class AlunoDesistente
        {
            public string UserId { get; set; }
            public string Name { get; set; }

            public double Engajamento { get; set; }
        }
        public class AlunoEngajamento
        {
            [JsonPropertyName("userId")]
            public string UserId { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("engajamento")]
            public double Engajamento { get; set; }
        }
        public async Task OnGetAsync(string curso)
        {
            Cursos = new Curso()
            {
                nomeCurso = curso
            };

            using var httpClient = new HttpClient();

            // Buscar quantAlunos do nomeCurso
            var responseAlunos = await httpClient.GetAsync($"https://localhost:7232/api/Moodle/alunos-por-curso/{curso}");
            if (responseAlunos.IsSuccessStatusCode)
            {
                var jsonAlunos = await responseAlunos.Content.ReadAsStringAsync();
                Cursos.usuarios = JsonSerializer.Deserialize<List<Usuario>>(jsonAlunos) ?? new();
            }
            else
            {
                Cursos.usuarios = new List<Usuario>();
                Cursos.quantAlunos = 0;
            }

            await Cursos.AtualizaSemanas();

            // Buscar dados de engajamento
            var responseEngajamento = await httpClient.GetAsync("https://localhost:7232/api/Mongo/engajamento-alunos");
            if (responseEngajamento.IsSuccessStatusCode)
            {
                var jsonEngajamento = await responseEngajamento.Content.ReadAsStringAsync();
                var todosAlunosEngajamento = JsonSerializer.Deserialize<List<AlunoEngajamento>>(jsonEngajamento) ?? new();

                AlunosEngajamento = todosAlunosEngajamento
                .Where(a => Cursos.usuarios.Any(al => al.user_id == a.UserId))
                .ToList();



            }
            else
            {
                AlunosEngajamento = new List<AlunoEngajamento>();
            }

            CauculaEngag();

            //  limiar de engajamento para considerar como desistente (30%)
            double limiarEngajamento = 30.0;

            // Filtra quantAlunos com engajamento abaixo do limiar
            PossiveisDesistentes = AlunosEngajamento
                .Where(a => a.Engajamento < limiarEngajamento)
                .Select(a => new AlunoDesistente
                {
                    UserId = a.UserId,
                    Name = a.Name,
                    Engajamento = a.Engajamento
                })
                .ToList();
        }


        private void CauculaEngag()
        {
            if (AlunosEngajamento.Any())
            {
                var total_ = AlunosEngajamento.Count;
                EngajamentoAlto = (int)((AlunosEngajamento.Count(l => l.Engajamento > 66) * 100.0) / total_);
                EngajamentoMedio = (int)((AlunosEngajamento.Count(l => l.Engajamento <= 66 && l.Engajamento >= 33) * 100.0) / total_);
                EngajamentoBaixo = (int)((AlunosEngajamento.Count(l => l.Engajamento < 33) * 100.0) / total_);
            }
            else
            {
                EngajamentoAlto = 0;
                EngajamentoMedio = 0;
                EngajamentoBaixo = 0;
            }
        }

    }
}
