using Backend.Models;
using Backend.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text;


namespace Backend.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<AlunoLogs> _collection;



        public class AlunoEngajamento
        {
            public string UserId { get; set; }
            public string Nome { get; set; }
            public double Engajamento { get; set; } // 0 a 100
        }

        public class AcaoQuantidade
        {
            public string Acao { get; set; }
            public int Quantidade { get; set; }
        }

        public class EngajamentoService
        {
            private readonly IMongoCollection<BsonDocument> _logs;



            public EngajamentoService(IMongoDatabase database)
            {
                _logs = database.GetCollection<BsonDocument>("logs");
            }

            public async Task<List<AlunoEngajamento>> CalcularEngajamentoAlunosAsync()
            {
                var pipeline = new[]
                {
        new BsonDocument("$unwind", "$logs"),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", new BsonDocument
                {
                    { "user_id", "$logs.user_id" },
                    { "nome", "$logs.name" },
                    { "acao", "$logs.action" },
                    { "target", "$logs.target"},
                    {"component", $"logs.component" }

                }
            },
            { "qtd", new BsonDocument("$sum", 1) }
        })
    };

                using var cursor = await _logs.AggregateAsync<BsonDocument>(pipeline);

                var resultados = await cursor.ToListAsync();

                var agrupados = resultados
                    .GroupBy(d => new
                    {
                        UserId = d["_id"].AsBsonDocument.TryGetValue("user_id", out var userId) ? userId.AsString : "undefined",
                        Nome = d["_id"].AsBsonDocument.TryGetValue("nome", out var nome) ? nome.AsString : "undefined"
                    })
                    .Select(g => new AlunoEngajamento
                    {
                        UserId = g.Key.UserId,
                        Nome = g.Key.Nome,
                        Engajamento = CalcularLES(g.Select(x => new AcaoQuantidade
                        {
                            Acao = x["_id"].AsBsonDocument.TryGetValue("acao", out var acao) ? acao.AsString : "undefined",
                            Quantidade = x["qtd"].ToInt32()
                        }).ToList())
                    })
                    .ToList();

                return agrupados;
            }



            private double CalcularLES(List<AcaoQuantidade> acoes)
            {
                int totalVisualizacao = Soma(acoes, new List<string> { "viewed" });
                int totalEntrega = Soma(acoes, new List<string> { "submitted", "uploaded" });
                int totalForum = Soma(acoes, new List<string> { "created", "uploaded", "viewed" });
                int totalAcesso = Soma(acoes, new List<string> { "viewed" });
                int totalAvaliacao = Soma(acoes, new List<string> { "graded", "reviewed" });
                int totalTempo = totalVisualizacao;

                double notaVisualizacao = NotaPorFaixa(totalVisualizacao, 500);
                double notaEntrega = NotaPorFaixa(totalEntrega, 20);
                double notaForum = NotaPorFaixa(totalForum, 30);
                double notaAcesso = NotaPorFaixa(totalAcesso, 100);
                double notaAvaliacao = NotaPorFaixa(totalAvaliacao, 20);
                double notaTempo = NotaPorFaixa(totalTempo, 500);

                double les =
                    notaEntrega * 0.30 +
                    notaVisualizacao * 0.20 +
                    notaForum * 0.15 +
                    notaTempo * 0.15 +
                    notaAcesso * 0.10 +
                    notaAvaliacao * 0.10;

                return Math.Round(les * 10, 2);
            }


            private int Soma(List<AcaoQuantidade> acoes, List<string> nomes)
            {
                return acoes.Where(a => nomes.Contains(a.Acao)).Sum(a => a.Quantidade);
            }

            private double NotaPorFaixa(int valor, int maxEsperado)
            {
                double nota = (double)valor / maxEsperado * 10;
                return Math.Min(nota, 10);
            }
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
                { "nomeCurso", "$_id" }
            })
            };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);

            var cursos = new List<string>();
            await resultado.ForEachAsync(doc =>
            {
                cursos.Add(doc["nomeCurso"].AsString);
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
                            { "nomeCurso", "$logs.course_fullname" },
                            { "quantAlunos", "$logs.user_id" }
                        }
                    }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$_id.nomeCurso" },
                    { "quantAlunos", new BsonDocument("$sum", 1) }
                }),
                new BsonDocument("$project", new BsonDocument
                {
                    { "_id", 0 },
                    { "nomeCurso", "$_id" },
                    { "quantAlunos", 1 }
                })
            };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);

            var cursos = new List<CursoComQtdAlunos>();
            await resultado.ForEachAsync(doc =>
            {
                cursos.Add(new CursoComQtdAlunos
                {
                    nomeCurso = doc["nomeCurso"].AsString,
                    quantAlunos = doc["quantAlunos"].AsInt32
                });
            });

            return cursos;
        }
        public async Task<List<Usuario>> GetAlunosPorCursoAsync(string nomeCurso)
        {
            var pipeline = new BsonDocument[]
            {
        new BsonDocument("$unwind", "$logs"),
        new BsonDocument("$match", new BsonDocument("logs.course_fullname", nomeCurso)),
        new BsonDocument("$group", new BsonDocument
        {
            { "_id", "$logs.user_id" },
            { "name", new BsonDocument("$first", "$logs.name") },
            { "user_lastaccess", new BsonDocument("$max", "$logs.user_lastaccess") }
        }),
        new BsonDocument("$project", new BsonDocument
        {
            { "_id", 0 },
            { "user_id", "$_id" },
            { "name", 1 },
            { "user_lastaccess", 1 }
        })
            };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);

            var alunos = new List<Usuario>();
            await resultado.ForEachAsync(doc =>
            {
                var aluno = new Usuario
                {
                    user_id = doc.GetValue("user_id", "").AsString,
                    name = doc.GetValue("name", "").AsString,
                    user_lastaccess = doc.GetValue("user_lastaccess").BsonType == BsonType.DateTime
    ? doc.GetValue("user_lastaccess").ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")
    : doc.GetValue("user_lastaccess").ToString(),

                };

                alunos.Add(aluno);
            });

            return alunos;
        }
        public async Task<List<CursoComAlunos>> GetAlunosPorTodosOsCursosAsync()
        {
            var pipeline = new[]
{
    new BsonDocument("$unwind", "$logs"),
    new BsonDocument("$group", new BsonDocument
    {
        { "_id", new BsonDocument
            {
                { "course_fullname", "$logs.course_fullname" },
                { "user_id", "$logs.user_id" }
            }
        },
        { "name", new BsonDocument("$first", "$logs.name") },
        { "ultimo_acesso", new BsonDocument("$max", "$logs.user_lastaccess") }
    }),
    new BsonDocument("$group", new BsonDocument
    {
        { "_id", "$_id.course_fullname" },
        { "alunos", new BsonDocument("$push", new BsonDocument
            {
                { "user_id", "$_id.user_id" },
                { "nome", "$name" }, // corrigido
                { "ultimo_acesso", "$ultimo_acesso" } // corrigido
            })
        }
    }),
    new BsonDocument("$project", new BsonDocument
    {
        { "_id", 0 },
        { "nomeCurso", "$_id" },
        { "alunos", 1 }
    })
};

            var cursos = new List<CursoComAlunos>();

            using var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline);

            while (await cursor.MoveNextAsync())
            {
                foreach (var doc in cursor.Current)
                {
                    var alunos = doc["alunos"].AsBsonArray.Select(a => new Usuario
                    {
                        user_id = a["user_id"].AsString,
                        name = a["nome"].AsString,
                        user_lastaccess = a["ultimo_acesso"].ToString() // <-- Ajustado
                    }).ToList();

                    cursos.Add(new CursoComAlunos
                    {
                        nomeCurso = doc.GetValue("nomeCurso", "").AsString,
                        usuarios = alunos
                    });
                }
            }

            return cursos;
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

        public async Task<List<LogUsuario>> GetLogsPorCursoAsync(string nomeCurso)
        {
            var pipeline = new[]
            {
        new BsonDocument("$unwind", "$logs"),
        new BsonDocument("$match", new BsonDocument("logs.course_fullname", nomeCurso)),
        new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$logs"))
    };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);

            var logs = new List<LogUsuario>();
            await resultado.ForEachAsync(doc =>
            {
                logs.Add(new LogUsuario
                {
                    user_id = doc.GetValue("user_id", "").IsBsonNull ? "" : doc["user_id"].AsString,
                    name = doc.GetValue("name", "").AsString,
                    date = doc.GetValue("date", "").AsString,
                    action = doc.GetValue("action", "").AsString,
                    target = doc.GetValue("target", "").AsString,
                    component = doc.GetValue("component", "").AsString,
                    course_fullname = doc.GetValue("course_fullname", "").AsString,
                    user_lastaccess = doc.GetValue("user_lastaccess", "").AsString,
                });
            });

            return logs;
        }


        public async Task<string> GerarResumoAlunoIAAsync(string userId)
        {
            // Pegar todos os logs do usuário
            var match = new BsonDocument("$match", new BsonDocument("user_id", userId));
            var unwind = new BsonDocument("$unwind", "$logs");
            var matchLogs = new BsonDocument("$match", new BsonDocument("logs.user_id", userId));
            var replaceRoot = new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$logs"));

            var pipeline = new List<BsonDocument> { unwind, matchLogs, replaceRoot };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var logs = await resultado.ToListAsync();

            if (!logs.Any())
                return "Usuário não encontrado ou sem interações.";

            var nome = logs.FirstOrDefault()?.GetValue("name", BsonNull.Value)?.AsString ?? "Nome não disponível";

            var cursoInteracoes = logs
                .GroupBy(l => l["course_fullname"].AsString)
                .ToDictionary(g => g.Key, g => g.Count());

            var componentes = logs
                .GroupBy(l => l["component"].AsString)
                .ToDictionary(g => g.Key, g => g.Count());

            var acoes = logs
                .GroupBy(l => l["action"].AsString)
                .ToDictionary(g => g.Key, g => g.Count());

            var diasAtivos = logs
                .Select(l => DateTime.Parse(l["date"].AsString).Date)
                .Distinct()
                .Count();

            var primeiroAcesso = logs
                .Min(l => DateTime.Parse(l["date"].AsString));

            var ultimoAcesso = logs
                .Max(l => DateTime.Parse(l["date"].AsString));

            var totalInteracoes = logs.Count;

            // Interações semanais
            var porSemana = logs
                .GroupBy(l =>
                {
                    var data = DateTime.Parse(l["date"].AsString);
                    var calendar = System.Globalization.CultureInfo.InvariantCulture.Calendar;
                    var week = calendar.GetWeekOfYear(data, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    return $"{data.Year}-W{week:D2}";
                })
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var componentesSemana = g.GroupBy(l => l["component"].AsString)
                                             .ToDictionary(c => c.Key, c => c.Count());
                    return new
                    {
                        Semana = g.Key,
                        Total = g.Count(),
                        Componentes = componentesSemana
                    };
                });

            // Construir o resumo
            var sb = new StringBuilder();
            sb.AppendLine($"Nome do Aluno: {nome}");
            sb.AppendLine($"User ID: {userId}");
            sb.AppendLine($"Total de Interações: {totalInteracoes}");
            sb.AppendLine($"Dias Ativos: {diasAtivos}");
            sb.AppendLine($"Primeiro Acesso: {primeiroAcesso:yyyy-MM-dd}");
            sb.AppendLine($"Último Acesso: {ultimoAcesso:yyyy-MM-dd}");
            sb.AppendLine();

            sb.AppendLine("Interações por nomeCurso:");
            foreach (var curso in cursoInteracoes)
                sb.AppendLine($"- {curso.Key}: {curso.Value} interações");

            sb.AppendLine("\nComponentes mais utilizados:");
            foreach (var comp in componentes.OrderByDescending(c => c.Value).Take(5))
                sb.AppendLine($"- {comp.Key}: {comp.Value}");

            sb.AppendLine("\nAções mais realizadas:");
            foreach (var acao in acoes.OrderByDescending(a => a.Value).Take(5))
                sb.AppendLine($"- {acao.Key}: {acao.Value}");

            sb.AppendLine("\nResumo Semanal:");
            foreach (var semana in porSemana)
            {
                sb.AppendLine($"Semana: {semana.Semana}");
                sb.AppendLine($"  Total de Interações: {semana.Total}");
                sb.AppendLine($"  Componentes:");
                foreach (var comp in semana.Componentes)
                    sb.AppendLine($"    - {comp.Key}: {comp.Value}");
            }

            return sb.ToString();
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
                return "nomeCurso não encontrado ou sem atividades.";

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

            // quantAlunos com apenas 1 acesso
            var alunos1Acesso = logs
                .GroupBy(l => l.user_id)
                .Count(g => g.Count() == 1);

            // quantAlunos inativos há mais de 30 dias
            var alunosInativos = logs
                .GroupBy(l => l.user_id)
                .Where(g => g.Max(l => DateTime.TryParse(l.date, out var d) ? d : DateTime.MinValue) < limite30dias)
                .Select(g => g.Key)
                .Count();

            // quantAlunos que nunca realizaram ações de engajamento
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
        $"nomeCurso: {nomeCurso}",
        $"Total de quantAlunos: {alunosUnicos}",
        $"Total de Acessos: {totalAcessos}",
        $"Média de Acessos por Aluno: {mediaAcessosPorAluno:F2}",
        $"Dias Ativos: {diasAtivos}",
        $"Duração Estimada do Uso: {duracaoDias} dias",
        $"quantAlunos Ativos nos Últimos 30 dias: {alunosAtivos30Dias}",
        $"quantAlunos Inativos >30 dias: {alunosInativos}",
        $"% quantAlunos com >5 Acessos: {percentualAlunosEngajados:F2}%",
        $"quantAlunos com apenas 1 acesso: {alunos1Acesso}",
        $"quantAlunos sem Engajamento: {alunosSemEngajamento}",
        $"Interações por Componente: {string.Join(", ", percentualPorComponente)}",
        $"Ações Mais Comuns: {string.Join(", ", acoesMaisComuns)}",
        $"Top Ações de Engajamento: {string.Join(", ", acoesEngajamentoTop)}",
        $"Ações Passivas: {totalPassivas}, Ativas: {totalAtivas}",
        $"quantAlunos com acesso em >1 dia: {alunosMultiplosDias}",
        $"Top 3 Usuários com Mais Acessos: {string.Join(", ", topUsuarios)}",
        $"Top 10 Piores Usuários (menos acessos): {string.Join(", ", pioresUsuarios)}",
        $"Média de dias entre 1º e último acesso por aluno: {mediaDiasPermanencia:F2} dias"
    });

            return resumoStr;
        }

        public async Task<List<Usuario>> GetUsuariosComUltimoAcessoAsync()
        {
            var pipeline = new BsonDocument[]
            {
        new BsonDocument("$unwind", "$logs"),
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

            var usuarios = new List<Usuario>();
            await resultado.ForEachAsync(doc =>
            {
                var userIdVal = doc.GetValue("user_id", BsonNull.Value);
                var nomeVal = doc.GetValue("nome", BsonNull.Value);
                var ultimoAcessoVal = doc.GetValue("ultimo_acesso", BsonNull.Value);

                usuarios.Add(new Usuario
                {
                    user_id = userIdVal.IsBsonNull ? "" : userIdVal.AsString,
                    name = nomeVal.IsBsonNull ? "" : nomeVal.AsString,
                    user_lastaccess = ultimoAcessoVal.BsonType == BsonType.DateTime
                        ? ultimoAcessoVal.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss")
                        : (ultimoAcessoVal.IsBsonNull ? "" : ultimoAcessoVal.ToString()),
                });
            });

            return usuarios;
        }



    }
}
