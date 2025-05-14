using Backend.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend
{
    public class MongoService
    {
        private readonly IMongoCollection<AlunoLogs> _collection;

        public MongoService(IConfiguration config)
        {
            var client = new MongoClient(config["MongoSettings:ConnectionString"]);
            var database = client.GetDatabase(config["MongoSettings:Database"]);
            _collection = database.GetCollection<AlunoLogs>("logs");
        }

        public async Task SalvarLogsAsync(string userId, List<LogUsuario> logs)
        {
            var documento = new AlunoLogs
            {
                user_id = userId,
                logs = logs
            };

            var filtro = Builders<AlunoLogs>.Filter.Eq(x => x.user_id, userId);
            var opcoes = new ReplaceOptions { IsUpsert = true };
            await _collection.ReplaceOneAsync(filtro, documento, opcoes);
        }

        public async Task<List<string>> GetCursosDistintosAsync()
        {
            var pipeline = new BsonDocument[]
            {
            new BsonDocument("$unwind", "$logs"),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$logs.course_fullname" }
            }),
            new BsonDocument("$project", new BsonDocument
            {
                { "_id", 0 },
                { "curso", "$_id" }
            })
            };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);

            var cursos = new List<string>();
            await resultado.ForEachAsync(doc =>
            {
                cursos.Add(doc["curso"].AsString);
            });

            return cursos;
        }

        public async Task<List<CursoComQtdAlunos>> GetCursosComQtdAlunosAsync()
        {
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$unwind", "$logs"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument
                        {
                            { "curso", "$logs.course_fullname" },
                            { "aluno", "$logs.user_id" }
                        }
                    }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$_id.curso" },
                    { "alunos", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "curso", "$_id" },
                    { "alunos", 1 }
                })
            };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);

            var cursos = new List<CursoComQtdAlunos>();
            await resultado.ForEachAsync(doc =>
            {
                cursos.Add(new CursoComQtdAlunos
                {
                    Curso = doc["curso"].AsString,
                    Alunos = doc["alunos"].AsInt32
                });
            });

            return cursos;
        }

    }
}
