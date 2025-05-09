using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Frontend.Models;


namespace Frontend.Pages
{
    public class IndexModel : PageModel
    {
        public List<Usuario> Usuarios { get; set; }

        public async Task OnGetAsync()
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://localhost:7232/api/Moodle/usuarios");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Usuarios = JsonSerializer.Deserialize<List<Usuario>>(json);
            }
            else
            {
                Usuarios = new List<Usuario>();
            }
        }
    }
}
