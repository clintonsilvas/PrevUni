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
        public async Task<List<string>> GetAlunosPorCursoAsync(string nomeCurso)
        {
            var pipeline = new BsonDocument[]
            {
        new BsonDocument("$match", new BsonDocument
        {
            { "logs.course_fullname", nomeCurso }
        }),
        new BsonDocument("$project", new BsonDocument
        {
            { "_id", 0 },
            { "user_id", "$_id" },  // ou "$logs.user_id" se preferir pegar do log
            { "nome", new BsonDocument("$arrayElemAt", new BsonArray { "$logs.name", 0 }) }
        })
            };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);

            var alunos = new List<string>();
            await resultado.ForEachAsync(doc =>
            {
                var nome = doc.GetValue("nome", "").AsString;
                alunos.Add(nome);
            });

            return alunos;
        }

        public class AcaoQuantidade
        {
            public string acao { get; set; }
            public int quantidade { get; set; }
        }

        public class UsuarioAcoes
        {
            public string userId { get; set; }
            public string nome { get; set; }
            public List<AcaoQuantidade> acoes { get; set; }
        }

        public class EngajamentoUsuario
        {
            public string Nome { get; set; }
            public string UserId { get; set; }
            public double Engajamento { get; set; } // 0-100
        }

        public class EngajamentoService
        {
            private readonly Dictionary<string, int> _pesos = new()
    {
        { "viewed", 1 },
        { "submitted", 5 },
        { "uploaded", 4 },
        { "graded", 4 },
        { "created", 3 },
        { "updated", 2 },
        { "deleted", 1 },
        { "started", 3 }
    };

            public List<EngajamentoUsuario> CalcularEngajamento(List<UsuarioAcoes> usuarios)
            {
                // Calcula pontuação bruta para todos
                var resultado = new List<(UsuarioAcoes usuario, int score)>();

                foreach (var usuario in usuarios)
                {
                    int score = 0;
                    foreach (var acao in usuario.acoes)
                    {
                        if (_pesos.TryGetValue(acao.acao, out var peso))
                        {
                            score += acao.quantidade * peso;
                        }
                    }
                    resultado.Add((usuario, score));
                }

                // Normaliza para 0-100
                int maxScore = resultado.Max(r => r.score);
                var engajamento = resultado.Select(r => new EngajamentoUsuario
                {
                    Nome = r.usuario.nome,
                    UserId = r.usuario.userId,
                    Engajamento = maxScore > 0 ? Math.Round((double)r.score / maxScore * 100, 2) : 0
                }).ToList();

                return engajamento;
            }
        }


        public async Task<List<UsuarioComAcoes>> GetAcoesPorUsuarioAsync()
        {
            var pipeline = new BsonDocument[]
            {
        new BsonDocument("$unwind", "$logs"),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", new BsonDocument
                {
                    { "user_id", "$logs.user_id" },
                    { "nome", "$logs.name" },
                    { "acao", "$logs.action" }
                }
            },
            { "qtd", new BsonDocument("$sum", 1) }
        }),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", new BsonDocument
                {
                    { "user_id", "$_id.user_id" },
                    { "nome", "$_id.nome" }
                }
            },
            { "acoes", new BsonDocument("$push", new BsonDocument
                {
                    { "acao", "$_id.acao" },
                    { "qtd", "$qtd" }
                })
            }
        }),
        new BsonDocument("$project", new BsonDocument
        {
            { "_id", 0 },
            { "user_id", "$_id.user_id" },
            { "nome", "$_id.nome" },
            { "acoes", 1 }
        })
            };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);

            var usuarios = new List<UsuarioComAcoes>();
            await resultado.ForEachAsync(doc =>
            {
                var user = new UsuarioComAcoes
                {
                    UserId = doc["user_id"].AsString,
                    Nome = doc["nome"].AsString,
                    Acoes = doc["acoes"]
                        .AsBsonArray
                        .Select(a => new AcaoQtd
                        {
                            Acao = a["acao"].AsString,
                            Quantidade = a["qtd"].AsInt32
                        }).ToList()
                };

                usuarios.Add(user);
            });

            return usuarios;
        }

        public class UsuarioComAcoes
        {
            public string UserId { get; set; }
            public string Nome { get; set; }
            public List<AcaoQtd> Acoes { get; set; }
        }

        public class AcaoQtd
        {
            public string Acao { get; set; }
            public int Quantidade { get; set; }
        }


        public async Task<List<LogUsuario>> GetLogsPorCursoAsync(string nomeCurso)
        {
            var pipeline = new BsonDocument[]
            {
        new BsonDocument("$unwind", "$logs"),
        new BsonDocument("$match", new BsonDocument("logs.course_fullname", nomeCurso)),
        new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$logs"))
            };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);

            var logs = new List<LogUsuario>();
            await resultado.ForEachAsync(doc =>
            {
                var log = new LogUsuario
                {
                    // Aqui trata o user_id para evitar erro ou null
                    user_id = doc.Contains("user_id") && !doc["user_id"].IsBsonNull ? doc["user_id"].AsString : "",

                    name = doc.GetValue("name", "").AsString,
                    date = doc.GetValue("date", "").AsString,
                    action = doc.GetValue("action", "").AsString,
                    target = doc.GetValue("target", "").AsString,
                    component = doc.GetValue("component", "").AsString,
                    course_fullname = doc.GetValue("course_fullname", "").AsString,
                    user_lastaccess = doc.GetValue("user_lastaccess", "").AsString,
                };

                logs.Add(log);
            });

            return logs;
        }

        public async Task<string> GerarResumoAlunoAsync(string userId)
        {
            var pipeline = new[]
            {
        new BsonDocument("$unwind", "$logs"),
        new BsonDocument("$match", new BsonDocument("logs.user_id", userId)),
        new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$logs")),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$user_id" },
            { "nome", new BsonDocument("$first", "$name") },
            { "ultimo_acesso", new BsonDocument("$max", "$user_lastaccess") },
            { "total_acessos", new BsonDocument("$sum", 1) },
            { "dias_ativos", new BsonDocument("$addToSet", new BsonDocument("$substr", new BsonArray { "$date", 0, 10 })) },
            { "interacoes_por_componente", new BsonDocument("$push", "$component") },
            { "cursos", new BsonDocument("$addToSet", "$course_fullname") }
        })
    };

            var result = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var doc = await result.FirstOrDefaultAsync();

            if (doc == null) return "Usuário não encontrado";

            // Agrupando contagem de componentes
            var componentes = doc["interacoes_por_componente"].AsBsonArray
                .GroupBy(x => x.AsString)
                .ToDictionary(g => g.Key, g => g.Count());

            var componentesStr = string.Join(",", componentes.Select(kv => $"{kv.Key}:{kv.Value}"));
            var cursosStr = string.Join(",", doc["cursos"].AsBsonArray.Select(c => c.AsString));

            var resumoStr = string.Join(" / ", new[]
            {
        $"ID: {doc["_id"]}",
        $"Nome: {doc["nome"]}",
        $"Último Acesso: {doc["ultimo_acesso"]}",
        $"Total de Acessos: {doc["total_acessos"]}",
        $"Dias Ativos: {doc["dias_ativos"].AsBsonArray.Count}",
        $"Interações por Componente: {componentesStr}",
        $"Cursos Acessados: {cursosStr}"
    });

            return resumoStr;
        }






    }

}
