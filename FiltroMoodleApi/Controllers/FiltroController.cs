using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FiltroMoodleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FiltroController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public FiltroController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("por-curso/{nomeCurso}")]
        public async Task<IActionResult> FiltrarPorCurso(string nomeCurso)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync("https://localhost:7145/api/dados");

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, "Erro ao buscar dados");

                var json = await response.Content.ReadAsStringAsync();
                var dados = JsonSerializer.Deserialize<MoodleResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (dados?.Logs == null)
                    return NotFound("Nenhum dado encontrado.");

                var filtrados = dados.Logs
                    .Where(log => log.CourseFullname.Contains(nomeCurso, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return Ok(filtrados);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}
