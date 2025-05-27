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
        public List<LogUsuario> LogsCurso { get; set; } = new();

        // Em Front.Pages/DashboardModel.cs

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
                Cursos.alunos = 0; // Define como zero se não conseguir buscar os alunos
            }

            // 2. Chamar o método AtualizaSemanas do objeto Cursos (que já está inicializado com o nome do curso)
            await Cursos.AtualizaSemanas();

        }

    }
}
