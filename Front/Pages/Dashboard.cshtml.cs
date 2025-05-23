using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Text.Json;

namespace Front.Pages
{
    public class DashboardModel : PageModel
    {
        public Curso Cursos { get; set; } = new();
        public List<string> Alunos { get; set; } = new();
        public List<LogUsuario> LogsCurso { get; set; } = new();

        public async Task OnGetAsync(string nome, int quantidadeAlunos)
        {
            Cursos = new Curso()
            {
                curso = nome,
                alunos = quantidadeAlunos
            };

            using var httpClient = new HttpClient();

            var responseAlunos = await httpClient.GetAsync($"https://localhost:7232/api/Moodle/alunos-por-curso/{nome}");
            if (responseAlunos.IsSuccessStatusCode)
            {
                var json = await responseAlunos.Content.ReadAsStringAsync();
                Alunos = JsonSerializer.Deserialize<List<string>>(json) ?? new();
            }

            await Cursos.AtualizaSemanas();

        }

    }
}
