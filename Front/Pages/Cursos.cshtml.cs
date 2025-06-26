using Front.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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

        public async Task<JsonResult> OnGetCursosComQuantAlunosAsync()
        {
            var cursos = await GetFromApiAsync<List<Curso>>(
                "https://localhost:7232/api/Moodle/cursos/com-alunos") ?? new();

            return new JsonResult(cursos
                .Select(c => new
                {
                    c.nomeCurso,
                    quantAlunos = c.quantAlunos
                })
                .OrderBy(c => c.nomeCurso)
                .ToList());
        }

        public async Task<JsonResult> OnGetEngajamentoCursoAsync(string nomeCurso)
        {
            var cursosComAlunos = await GetFromApiAsync<List<Curso>>(
                "https://localhost:7232/api/Moodle/curso/alunos") ?? new();

            var cursoAlunos = cursosComAlunos.FirstOrDefault(c => c.nomeCurso == nomeCurso);
            if (cursoAlunos == null || cursoAlunos.usuarios == null || !cursoAlunos.usuarios.Any())
            {
                return new JsonResult(new { engagAlto = 0, engagMedio = 0, engagBaixo = 0 });
            }

            var engajamentos = await GetFromApiAsync<List<AlunoEngajamento>>(
                $"https://localhost:7232/api/Engajamento/curso/{Uri.EscapeDataString(nomeCurso)}") ?? new();

            var alunosEng = engajamentos
                .Where(e => cursoAlunos.usuarios.Any(u => u.user_id == e.UserId))
                .ToList();

            int total = alunosEng.Count;
            int engagAlto = 0;
            int engagMedio = 0;
            int engagBaixo = 0; // DECLARAÇÃO CORRETA

            if (total > 0)
            {
                engagAlto = (int)(alunosEng.Count(e => e.Engajamento > 66) * 100.0 / total);
                engagMedio = (int)(alunosEng.Count(e => e.Engajamento is >= 33 and <= 66) * 100.0 / total);
                engagBaixo = 100 - engagAlto - engagMedio; // USO CORRETO
            }

            return new JsonResult(new
            {
                engagAlto,
                engagMedio,
                engagBaixo
            });
        }

        private async Task<T?> GetFromApiAsync<T>(string url)
        {
            try
            {
                var res = await _httpClient.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Erro ao buscar da API: {res.StatusCode} - {await res.Content.ReadAsStringAsync()}");
                    return default;
                }

                var json = await res.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exceção durante a chamada da API: {ex.Message}");
                return default;
            }
        }
    }
}