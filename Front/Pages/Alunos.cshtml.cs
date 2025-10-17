using Front.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Front.Pages
{
    public class AlunosModel : PageModel
    {
        public List<Usuario> users = new List<Usuario>();
        private readonly IConfiguration _configuration;
        public AlunosModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task OnGet()
        {
            using var httpClient = new HttpClient();

            // Buscar quantAlunos do nomeCurso
            var responseAlunos = await httpClient.GetAsync($"{_configuration["ApiUrl"]}/api/Unifenas/usuarios");

            var jsonAlunos = await responseAlunos.Content.ReadAsStringAsync();
            users = JsonSerializer.Deserialize<List<Usuario>>(jsonAlunos) ?? new();

        }
    }
}
