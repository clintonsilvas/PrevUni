using Front.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Front.Pages
{
    public class AlunosModel : PageModel
    {
        public List<Usuario> users = new List<Usuario>();
        public async Task OnGet()
        {
            using var httpClient = new HttpClient();

            // Buscar quantAlunos do nomeCurso
            var responseAlunos = await httpClient.GetAsync($"https://localhost:7232/api/Unifenas/usuarios");

            var jsonAlunos = await responseAlunos.Content.ReadAsStringAsync();
            users = JsonSerializer.Deserialize<List<Usuario>>(jsonAlunos) ?? new();

        }
    }
}
