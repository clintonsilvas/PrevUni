using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using PrevUni.Models;
using PrevUni.Services;
using System.Text.Json;

namespace PrevUni.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportarController : ControllerBase
    {
        private readonly AlunoService _alunoService;
        private readonly IHttpClientFactory _httpClientFactory;

        public ImportarController(AlunoService alunoService, IHttpClientFactory httpClientFactory)
        {
            _alunoService = alunoService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public async Task<IActionResult> ImportarAlunos()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("http://localhost:5095/api/alunosfake");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Erro ao buscar alunos");

            var json = await response.Content.ReadAsStringAsync();
            var alunos = JsonSerializer.Deserialize<List<Aluno>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            await _alunoService.InserirAlunosAsync(alunos);

            return Ok(new { Mensagem = "Alunos importados com sucesso", Total = alunos.Count });
        }

        [HttpGet("testar-mongo")]
        public IActionResult TestarMongo([FromServices] IConfiguration config)
        {
            try
            {
                var client = new MongoClient(config["MongoDB:ConnectionString"]);
                var database = client.GetDatabase(config["MongoDB:DatabaseName"]);
                var colecao = database.GetCollection<BsonDocument>(config["MongoDB:AlunosCollectionName"]);

                var count = colecao.CountDocuments(FilterDefinition<BsonDocument>.Empty);
                return Ok($"Conexão bem-sucedida. Total de documentos: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao conectar: {ex.Message}");  // Log adicional
                return StatusCode(500, $"Erro ao conectar: {ex.Message}");
            }
        }


    }

}
