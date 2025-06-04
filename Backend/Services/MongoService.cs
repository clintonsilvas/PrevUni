using Backend.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Backend.Services
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
        public async Task<List<AlunoResumo>> GetAlunosPorCursoAsync(string nomeCurso)
        {
            var pipeline = new BsonDocument[]
            {
        new BsonDocument("$unwind", "$logs"),
        new BsonDocument("$match", new BsonDocument("logs.course_fullname", nomeCurso)),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$logs.user_id" },
            { "nome", new BsonDocument("$first", "$logs.name") },
            { "ultimo_acesso", new BsonDocument("$max", "$logs.user_lastaccess") }
        }),
        new BsonDocument("$project", new BsonDocument
        {
            { "_id", 0 },
            { "user_id", "$_id" },
            { "nome", 1 },
            { "ultimo_acesso", 1 }
        })
            };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);

            var alunos = new List<AlunoResumo>();
            await resultado.ForEachAsync(doc =>
            {
                var aluno = new AlunoResumo
                {
                    user_id = doc.GetValue("user_id", "").AsString,
                    nome = doc.GetValue("nome", "").AsString,
                    ultimo_acesso = doc.GetValue("ultimo_acesso").BsonType == BsonType.DateTime
    ? doc.GetValue("ultimo_acesso").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")
    : doc.GetValue("ultimo_acesso").ToString(),

                    total_acessos = 0,
                    dias_ativos = 0,
                    interacoes_por_componente = new Dictionary<string, int>(),
                    cursos_acessados = new List<string>()
                };

                alunos.Add(aluno);
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

        public async Task<string> GerarResumoAlunoIAAsync(string userId)
        {
            var pipeline = new[]
            {
        new BsonDocument("$unwind", "$logs"),
        new BsonDocument("$match", new BsonDocument("logs.user_id", userId)),
        new BsonDocument("$addFields", new BsonDocument("logs", new BsonDocument
        {
            { "$mergeObjects", new BsonArray
                {
                    "$logs",
                    new BsonDocument
                    {
                        { "dateObj", new BsonDocument("$toDate", "$logs.date") }
                    }
                }
            }
        })),
        new BsonDocument("$addFields", new BsonDocument("logs", new BsonDocument
        {
            { "$mergeObjects", new BsonArray
                {
                    "$logs",
                    new BsonDocument("yearWeek", new BsonDocument("$dateToString", new BsonDocument
                    {
                        { "date", "$logs.dateObj" },
                        { "format", "%Y-W%V" },
                        { "timezone", "UTC" }
                    }))
                }
            }
        })),
        new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$logs")),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", new BsonDocument
                {
                    { "course", "$course_fullname" },
                    { "yearWeek", "$yearWeek" }
                }
            },
            { "interacoes_por_componente", new BsonDocument("$push", "$component") },
            { "total", new BsonDocument("$sum", 1) },
            { "ultimo_acesso", new BsonDocument("$max", "$user_lastaccess") }
        }),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$_id.course" },
            { "semanas", new BsonDocument("$push", new BsonDocument
                {
                    { "week", "$_id.yearWeek" },
                    { "total", "$total" },
                    { "interacoes_por_componente", "$interacoes_por_componente" },
                    { "ultimo_acesso", "$ultimo_acesso" }
                })
            }
        })
    };

            var result = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var cursos = await result.ToListAsync();

            if (!cursos.Any())
                return "Usuário não encontrado ou sem interações.";

            var resumoList = new List<string>();

            foreach (var cursoDoc in cursos)
            {
                var curso = cursoDoc["_id"].AsString;
                var semanas = cursoDoc["semanas"].AsBsonArray;

                resumoList.Add($"Curso: {curso}");

                foreach (var semanaDoc in semanas.OrderBy(s => s["week"].AsString))
                {
                    var semana = semanaDoc["week"].AsString;
                    var total = semanaDoc["total"].AsInt32;
                    var ultimoAcesso = semanaDoc["ultimo_acesso"].ToString();

                    // Agrupando interações por componente
                    var componentes = semanaDoc["interacoes_por_componente"].AsBsonArray
                        .GroupBy(x => x.AsString)
                        .ToDictionary(g => g.Key, g => g.Count());

                    var compStr = string.Join(", ", componentes.Select(kv => $"{kv.Key}: {kv.Value}"));

                    resumoList.Add($"  Semana: {semana}");
                    resumoList.Add($"    Total de Interações: {total}");
                    resumoList.Add($"    Último Acesso: {ultimoAcesso}");
                    resumoList.Add($"    Componentes: {compStr}");
                }
            }

            return string.Join(Environment.NewLine, resumoList);
        }


        public async Task<BsonDocument> GerarResumoAlunoAsync(string userId)
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

            if (doc == null) return null;

            // Agrupando contagem de componentes
            var componentes = doc["interacoes_por_componente"].AsBsonArray
                .GroupBy(x => x.AsString)
                .ToDictionary(g => g.Key, g => g.Count());

            var resumo = new BsonDocument
    {
        { "user_id", doc["_id"] },
        { "nome", doc["nome"] },
        { "ultimo_acesso", doc["ultimo_acesso"] },
        { "total_acessos", doc["total_acessos"] },
        { "dias_ativos", doc["dias_ativos"].AsBsonArray.Count },
        { "interacoes_por_componente", new BsonDocument(componentes) },
        { "cursos_acessados", doc["cursos"].AsBsonArray }
    };

            return resumo;
        }

        public async Task<string> GerarResumoCursoIAAsync(string nomeCurso)
        {
            var logs = await GetLogsPorCursoAsync(nomeCurso);
            if (logs == null || logs.Count == 0)
                return "Curso não encontrado ou sem atividades.";

            var totalAcessos = logs.Count;
            var alunosUnicos = logs.Select(l => l.user_id).Distinct().Count();

            var interacoesPorComponente = logs
                .GroupBy(l => l.component)
                .ToDictionary(g => g.Key, g => g.Count());

            double mediaAcessosPorAluno = alunosUnicos > 0 ? (double)totalAcessos / alunosUnicos : 0;

            var datasDistintas = logs
                .Select(l => l.date.Length >= 10 ? l.date.Substring(0, 10) : l.date)
                .Distinct()
                .ToList();

            int diasAtivos = datasDistintas.Count;

            // Estimativa de tempo de uso: intervalo entre primeiro e último acesso
            DateTime? primeiroAcesso = logs
                .Select(l => DateTime.TryParse(l.date, out var d) ? d : (DateTime?)null)
                .Where(d => d.HasValue)
                .OrderBy(d => d.Value)
                .FirstOrDefault();

            DateTime? ultimoAcesso = logs
                .Select(l => DateTime.TryParse(l.date, out var d) ? d : (DateTime?)null)
                .Where(d => d.HasValue)
                .OrderByDescending(d => d.Value)
                .FirstOrDefault();

            int duracaoDias = 0;
            if (primeiroAcesso.HasValue && ultimoAcesso.HasValue)
                duracaoDias = (ultimoAcesso.Value - primeiroAcesso.Value).Days + 1;

            DateTime limite30dias = DateTime.UtcNow.AddDays(-30);
            var alunosAtivos30Dias = logs
                .GroupBy(l => l.user_id)
                .Where(g => g.Max(l => DateTime.TryParse(l.date, out var d) ? d : DateTime.MinValue) >= limite30dias)
                .Select(g => g.Key)
                .Count();

            var alunosComMaisDe5Acessos = logs
                .GroupBy(l => l.user_id)
                .Count(g => g.Count() > 5);

            double percentualAlunosEngajados = alunosUnicos > 0 ? (double)alunosComMaisDe5Acessos / alunosUnicos * 100 : 0;

            var acoesMaisComuns = logs
                .GroupBy(l => l.action)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key}:{g.Count()}")
                .ToList();

            var acoesEngajamento = new[] { "submitted", "uploaded", "graded", "created" };
            var acoesEngajamentoTop = logs
                .Where(l => acoesEngajamento.Contains(l.action))
                .GroupBy(l => l.action)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key}:{g.Count()}")
                .ToList();

            var alunosMultiplosDias = logs
                .GroupBy(l => l.user_id)
                .Count(g => g.Select(l => l.date.Substring(0, 10)).Distinct().Count() > 1);

            var totalInteracoes = interacoesPorComponente.Values.Sum();
            var percentualPorComponente = interacoesPorComponente
                .Select(kv => $"{kv.Key}:{(kv.Value * 100.0 / totalInteracoes):F2}%")
                .ToList();

            var topUsuarios = logs
                .GroupBy(l => new { l.user_id, l.name })
                .OrderByDescending(g => g.Count())
                .Take(3)
                .Select(g => $"{g.Key.name} ({g.Count()})")
                .ToList();

            // NOVOS INSIGHTS

            // Top 10 piores alunos (menos acessos)
            var pioresUsuarios = logs
                .GroupBy(l => new { l.user_id, l.name })
                .OrderBy(g => g.Count())
                .Take(10)
                .Select(g => $"{g.Key.name} ({g.Count()})")
                .ToList();

            // Alunos com apenas 1 acesso
            var alunos1Acesso = logs
                .GroupBy(l => l.user_id)
                .Count(g => g.Count() == 1);

            // Alunos inativos há mais de 30 dias
            var alunosInativos = logs
                .GroupBy(l => l.user_id)
                .Where(g => g.Max(l => DateTime.TryParse(l.date, out var d) ? d : DateTime.MinValue) < limite30dias)
                .Select(g => g.Key)
                .Count();

            // Alunos que nunca realizaram ações de engajamento
            var alunosSemEngajamento = logs
                .GroupBy(l => l.user_id)
                .Count(g => !g.Any(l => acoesEngajamento.Contains(l.action)));

            // Ações passivas x ativas
            var acoesPassivas = new[] { "viewed", "read", "accessed" };
            var totalAtivas = logs.Count(l => acoesEngajamento.Contains(l.action));
            var totalPassivas = logs.Count(l => acoesPassivas.Contains(l.action));

            // Média de dias entre 1º e último acesso por aluno
            var mediasDiasPorAluno = logs
                .GroupBy(l => l.user_id)
                .Select(g =>
                {
                    var datas = g
                        .Select(l => DateTime.TryParse(l.date, out var d) ? d : (DateTime?)null)
                        .Where(d => d.HasValue)
                        .Select(d => d.Value)
                        .OrderBy(d => d)
                        .ToList();

                    return datas.Count > 1 ? (datas.Last() - datas.First()).Days : 0;
                })
                .ToList();

            double mediaDiasPermanencia = mediasDiasPorAluno.Count > 0 ? mediasDiasPorAluno.Average() : 0;

            var resumoStr = string.Join(" / ", new[]
            {
        $"Curso: {nomeCurso}",
        $"Total de Alunos: {alunosUnicos}",
        $"Total de Acessos: {totalAcessos}",
        $"Média de Acessos por Aluno: {mediaAcessosPorAluno:F2}",
        $"Dias Ativos: {diasAtivos}",
        $"Duração Estimada do Uso: {duracaoDias} dias",
        $"Alunos Ativos nos Últimos 30 dias: {alunosAtivos30Dias}",
        $"Alunos Inativos >30 dias: {alunosInativos}",
        $"% Alunos com >5 Acessos: {percentualAlunosEngajados:F2}%",
        $"Alunos com apenas 1 acesso: {alunos1Acesso}",
        $"Alunos sem Engajamento: {alunosSemEngajamento}",
        $"Interações por Componente: {string.Join(", ", percentualPorComponente)}",
        $"Ações Mais Comuns: {string.Join(", ", acoesMaisComuns)}",
        $"Top Ações de Engajamento: {string.Join(", ", acoesEngajamentoTop)}",
        $"Ações Passivas: {totalPassivas}, Ativas: {totalAtivas}",
        $"Alunos com acesso em >1 dia: {alunosMultiplosDias}",
        $"Top 3 Usuários com Mais Acessos: {string.Join(", ", topUsuarios)}",
        $"Top 10 Piores Usuários (menos acessos): {string.Join(", ", pioresUsuarios)}",
        $"Média de dias entre 1º e último acesso por aluno: {mediaDiasPermanencia:F2} dias"
    });

            return resumoStr;
        }





    }
}
