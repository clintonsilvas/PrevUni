using MongoDB.Driver;
using Microsoft.Extensions.Options;
using PrevUni.Models;
namespace PrevUni.Services
{
    public class AlunoService
    {
        private readonly IMongoCollection<Aluno> _alunos;

        public AlunoService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = client.GetDatabase(config["MongoDB:DatabaseName"]);
            _alunos = database.GetCollection<Aluno>(config["MongoDB:AlunosCollectionName"]);
        }
        public async Task InserirAlunosAsync(List<Aluno> alunos)
        {
            await _alunos.InsertManyAsync(alunos);
        }

        public async Task<List<Aluno>> ListarAlunosAsync()
        {
            return await _alunos.Find(_ => true).ToListAsync();
        }

    }
}
