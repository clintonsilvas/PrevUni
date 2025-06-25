using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Backend.Models;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/configuracao-engajamento")]
    public class ConfiguracaoEngajamentoController : ControllerBase
    {
        private readonly IMongoDatabase _database;

        public ConfiguracaoEngajamentoController(IMongoClient client)
        {
            _database = client.GetDatabase("SeuNomeDoBanco");
        }

        [HttpGet]
        public async Task<IActionResult> ObterConfiguracao()
        {
            var collection = _database.GetCollection<ConfiguracaoEngajamento>("configuracoesEngajamento");
            var config = await collection.Find(_ => true).FirstOrDefaultAsync();
            return Ok(config ?? new ConfiguracaoEngajamento());
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarConfiguracao([FromBody] ConfiguracaoEngajamento novaConfig)
        {
            double somaPesos = novaConfig.PesoVisualizacao +
                               novaConfig.PesoForum +
                               novaConfig.PesoEntrega +
                               novaConfig.PesoQuiz +
                               novaConfig.PesoAvaliacao;

            if (Math.Abs(somaPesos - 1.0) > 0.001)
            {
                return BadRequest("A soma dos pesos deve ser igual a 1.0");
            }

            var collection = _database.GetCollection<ConfiguracaoEngajamento>("configuracoesEngajamento");
            await collection.DeleteManyAsync(_ => true);
            await collection.InsertOneAsync(novaConfig);
            return Ok("Configuração atualizada com sucesso");
        }
    }
}
