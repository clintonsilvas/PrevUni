using Front.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Front.Pages
{
    public class CursosModel : PageModel
    {

        public List<Curso> Cursos { get; private set; } = new();


        private readonly HttpClient _httpClient;
        public CursosModel(HttpClient httpClient) => _httpClient = httpClient;

        public class AlunoEngajamento
        {
            [JsonPropertyName("userId")] public string UserId { get; set; } = "";
            [JsonPropertyName("name")] public string Nome { get; set; } = "";
            [JsonPropertyName("engajamento")] public double Engajamento { get; set; }
        }

        public Task OnGetAsync() => Task.CompletedTask;


        public async Task<PartialViewResult> OnGetCarregamentoAsync()
        {
            Cursos = await CarregarCursosComEngajamentoAsync();
            return Partial("Cursos/_Listagem_de_Cursos", Cursos.OrderBy(c => c.nomeCurso).ToList());
        }


        private async Task<List<Curso>> CarregarCursosComEngajamentoAsync()
        {
            var cursosComAlunos = await GetFromApiAsync<List<Curso>>(
                "https://localhost:7232/api/Moodle/curso/alunos") ?? new();

            var cursos = await GetFromApiAsync<List<Curso>>(
                "https://localhost:7232/api/Moodle/cursos/com-alunos") ?? new();

            var engajamentos = await GetFromApiAsync<List<AlunoEngajamento>>(
                "https://localhost:7232/api/Mongo/engajamento-alunos") ?? new();

            foreach (var c in cursos)
            {
                var alunos = cursosComAlunos.FirstOrDefault(l => l.nomeCurso == c.nomeCurso);
                if (alunos is null) continue;

                var alunosEng = engajamentos
                    .Where(a => alunos.usuarios.Any(u => u.user_id == a.UserId))
                    .ToList();

                var total = alunosEng.Count;
                if (total == 0) continue;

                c.engagAlto = (int)(alunosEng.Count(e => e.Engajamento > 66) * 100.0 / total);
                c.engagMedio = (int)(alunosEng.Count(e => e.Engajamento is >= 33 and <= 66) * 100.0 / total);
                c.engagBaixo = 100 - c.engagAlto - c.engagMedio;
            }

            return cursos;
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
