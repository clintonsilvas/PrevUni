using Front.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Front.Pages
{
    public class CursosModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public List<Curso> Cursos { get; private set; } = new();

        public CursosModel(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public class AlunoEngajamento
        {
            [JsonPropertyName("userId")] public string UserId { get; set; } = "";
            [JsonPropertyName("name")] public string Nome { get; set; } = "";
            [JsonPropertyName("engajamento")] public double Engajamento { get; set; }
        }

        public Task OnGetAsync() => Task.CompletedTask;

        public async Task<JsonResult> OnGetCursosComQuantAlunosAsync()
        {
            var url = $"{_configuration["ApiUrl"]}/api/Moodle/cursos/com-alunos";
            var cursos = await GetFromApiAsync<List<CursoDto>>(
                $"{_configuration["ApiUrl"]}/api/Moodle/cursos/com-alunos"
            ) ?? new();

            var resultado = cursos
                .Select(c => new { c.nomeCurso, c.quantAlunos })
                .OrderBy(c => c.nomeCurso)
                .ToList();

            return new JsonResult(resultado);
        }
        public class CursoDto
        {
            public string nomeCurso { get; set; }
            public int quantAlunos { get; set; }
        }

        public async Task<JsonResult> OnGetEngajamentoCursoAsync(string nomeCurso)
        {
            var cursosComAlunos = await GetFromApiAsync<List<Curso>>(
                $"{_configuration["ApiUrl"]}/api/Moodle/curso/alunos") ?? new();

            var cursoAlunos = cursosComAlunos.FirstOrDefault(c => c.nomeCurso == nomeCurso);

            if (cursoAlunos == null || cursoAlunos.usuarios == null || !cursoAlunos.usuarios.Any())
            {
                return new JsonResult(new { engagAlto = 0, engagMedio = 0, engagBaixo = 0 });
            }

            var engajamentos = await GetFromApiAsync<List<AlunoEngajamento>>(
                $"{_configuration["ApiUrl"]}/api/Engajamento/curso/{Uri.EscapeDataString(nomeCurso)}") ?? new();

            var alunosEng = engajamentos
                .Where(e => cursoAlunos.usuarios.Any(u => u.user_id == e.UserId))
                .ToList();

            int total = alunosEng.Count;
            int engagAlto = 0, engagMedio = 0, engagBaixo = 0;

            if (total > 0)
            {
                engagAlto = alunosEng.Count(e => e.Engajamento > 66) * 100 / total;
                engagMedio = alunosEng.Count(e => e.Engajamento >= 33 && e.Engajamento <= 66) * 100 / total;
                engagBaixo = 100 - engagAlto - engagMedio;
            }

            return new JsonResult(new { engagAlto, engagMedio, engagBaixo });
        }

        private async Task<T?> GetFromApiAsync<T>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erro API: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    return default;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção ao chamar API: {ex.Message}");
                return default;
            }
        }
    }
}
