using Front.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Front.Pages
{
    public class CursosModel : PageModel
    {
        public List<Curso> Cursos { get; set; } = new();
        public List<AlunoEngajamento> Engajamentos { get; set; } = new();

        private readonly HttpClient _httpClient;




        public CursosModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

        public async Task OnGetAsync()
        {
            var cursosComAlunos = await GetFromApiAsync<List<Curso>>(
                "https://localhost:7232/api/Moodle/curso/alunos") ?? new();

            Cursos = await GetFromApiAsync<List<Curso>>(
                "https://localhost:7232/api/Moodle/cursos/com-alunos") ?? new();

            Engajamentos = await GetFromApiAsync<List<AlunoEngajamento>>(
                "https://localhost:7232/api/Mongo/engajamento-alunos") ?? new();

            foreach (var c in Cursos)
            {
                var alunos = cursosComAlunos.Where(l => l.curso == c.curso).FirstOrDefault();

                var alunosEngajamento = Engajamentos
                    .Where(a => alunos.usuarios.Any(u => u.user_id == a.UserId))
                    .ToList();

                var total = alunosEngajamento.Count;

                c.engagAlto = total == 0 ? 0 : (int)(alunosEngajamento.Count(e => e.Engajamento > 66) * 100.0 / total);
                c.engagMedio = total == 0 ? 0 : (int)(alunosEngajamento.Count(e => e.Engajamento <= 66 && e.Engajamento >= 33) * 100.0 / total);
                c.engagBaixo = total == 0 ? 0 : (int)(alunosEngajamento.Count(e => e.Engajamento < 33) * 100.0 / total);
            }
        }






        private async Task<T?> GetFromApiAsync<T>(string url)
        {
            var res = await _httpClient.GetAsync(url);
            if (!res.IsSuccessStatusCode) return default;

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
