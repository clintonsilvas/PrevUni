using Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class CursoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CursoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var cursos = await client.GetFromJsonAsync<List<Curso>>("http://localhost:7204/api/cursos");

            return View(cursos);
        }

        public IActionResult Disciplinas(string nomeCurso)
        {
            // Simulação: Pega os dados em memória, depois pode buscar de verdade
            var curso = new Curso
            {
                Nome = nomeCurso,
                Disciplinas = new List<Disciplina>
            {
                new Disciplina { Nome = "Matemática", Professor = "Prof. Ana" },
                new Disciplina { Nome = "História", Professor = "Prof. João" }
            }
            };

            return View(curso);
        }
    }
}
