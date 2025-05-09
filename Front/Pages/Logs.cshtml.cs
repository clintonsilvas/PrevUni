using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Front.Models;
using System.Text.Json;

namespace Front.Pages
{
    public class LogsModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string userId { get; set; }

        public List<LogUsuario> Logs { get; set; } = new();
        public string UsuarioNome { get; set; } = "";

        public async Task OnGetAsync()
        {
            if (string.IsNullOrEmpty(userId))
                return;

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://localhost:7232/api/Moodle/logs/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                Logs = JsonSerializer.Deserialize<List<LogUsuario>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<LogUsuario>();
                UsuarioNome = Logs.FirstOrDefault()?.name ?? "Desconhecido";
            }
        }
    }
}
