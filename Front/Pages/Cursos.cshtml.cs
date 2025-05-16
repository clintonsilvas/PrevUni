using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace Front.Pages
{
    public class CursosModel : PageModel
    {
        public List<string> Cursos { get; set; } = new();

        public async Task OnGetAsync()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://localhost:7232/api/Moodle/cursos");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Cursos = JsonSerializer.Deserialize<List<string>>(json) ?? new();
            }
        }
    }
}
