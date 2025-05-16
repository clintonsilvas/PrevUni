using Front.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Front.Pages
{
    public class CursosModel : PageModel
    {
        public List<Cursos> Cursos { get; set; } = new();

        public async Task OnGetAsync()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://localhost:7232/api/Moodle/cursos/com-alunos");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Cursos = JsonSerializer.Deserialize<List<Cursos>>(json) ?? new();
            }
        }
    }
}
