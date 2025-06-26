using Backend.Models;
using Backend.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Globalization;
using System.Text;
using System.Text.Json;


namespace Backend.Services
{
    public class MongoService
    {
        private readonly IMongoCollection<AlunoLogs> _collection;



        public class AlunoEngajamento
        {
            public string UserId { get; set; }
            public string Nome { get; set; }
            public string CursoNome { get; set; }
            public double Engajamento { get; set; } // 0 a 100
        }

        public class AcaoQuantidade
        {
            public string Component { get; set; }
            public string Target { get; set; }
            public string Action { get; set; }
            public int Quantidade { get; set; }
        }



        public class EngajamentoService
        {
            private readonly IMongoCollection<BsonDocument> _logs;
            private readonly IMongoCollection<ConfiguracaoEngajamento> _configCollection;
            private readonly IConfiguration _configuration;

            public EngajamentoService(IMongoDatabase database, IConfiguration configuration)
            {
                _logs = database.GetCollection<BsonDocument>("logs");
                _configCollection = database.GetCollection<ConfiguracaoEngajamento>("configuracoesEngajamento");
                _configuration = configuration;
            }
            // Lê o arquivo config.json para obter os pesos de engajamento
            public ConfiguracaoEngajamento CarregarConfiguracao()
            {
                var section = _configuration.GetSection("ConfiguracaoEngajamento");

                return new ConfiguracaoEngajamento
                {
                    PesoVisualizacao = section.GetValue<double>("PesoVisualizacao"),
                    PesoForum = section.GetValue<double>("PesoForum"),
                    PesoEntrega = section.GetValue<double>("PesoEntrega"),
                    PesoQuiz = section.GetValue<double>("PesoQuiz"),
                    PesoAvaliacao = section.GetValue<double>("PesoAvaliacao"),
                };
            }

            // Calcula o engajamento dos alunos de um curso específico
            public async Task<List<AlunoEngajamento>> CalcularEngajamentoAlunosPorCursoAsync(string nomeCurso)
            {
                // Carrega a configuração dos pesos do arquivo
                var config = CarregarConfiguracao(); // <- corrigido: estava usando ConfiguracaoEngajamento.LoadFromFile, mas você já tem esse método

                // Define o pipeline de agregação do MongoDB para analisar logs
                var pipeline = new[]
                {
            // Quebra cada item do array "logs" em documentos individuais
            new BsonDocument("$unwind", "$logs"),

            // Filtra apenas os logs que correspondem ao nome do curso especificado
            new BsonDocument("$match", new BsonDocument("logs.course_fullname", nomeCurso)),

            // Agrupa os logs por aluno e tipo de ação
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument
                    {
                        { "user_id", "$logs.user_id" },
                        { "nome", "$logs.name" },
                        { "cursoNome", "$logs.course_fullname" },
                        { "component", "$logs.component" },
                        { "target", "$logs.target" },
                        { "action", "$logs.action" }
                    }
                },
                // Conta quantas vezes essa combinação ocorreu
                { "qtd", new BsonDocument("$sum", 1) }
            })
        };

                // Executa a agregação no MongoDB
                using var cursor = await _logs.AggregateAsync<BsonDocument>(pipeline);

                // Transforma os resultados em uma lista
                var resultados = await cursor.ToListAsync();

                // Agrupa os resultados por aluno
                var agrupados = resultados
                    .GroupBy(d => new
                    {
                        UserId = d["_id"]["user_id"].AsString,
                        Nome = d["_id"]["nome"].AsString,
                        CursoNome = d["_id"]["cursoNome"].AsString
                    })
                    .Select(g => new AlunoEngajamento
                    {
                        UserId = g.Key.UserId,
                        Nome = g.Key.Nome,
                        CursoNome = g.Key.CursoNome,

                        // Calcula o LES (índice de engajamento) com base nas ações do aluno e nos pesos
                        Engajamento = CalcularLES(g.Select(x => new AcaoQuantidade
                        {
                            Component = x["_id"]["component"].AsString,
                            Target = x["_id"]["target"].AsString,
                            Action = x["_id"]["action"].AsString,
                            Quantidade = x["qtd"].ToInt32()
                        }).ToList(), config)
                    })
                    .ToList();

                return agrupados;
            }

            // Calcula o índice LES de um aluno com base nas ações e na configuração de pesos
            private double CalcularLES(List<AcaoQuantidade> acoes, ConfiguracaoEngajamento config)
            {
                // Conta quantas vezes o aluno fez cada tipo de ação
                int totalVisualizacoes = Soma(acoes, "core", "course", "viewed");
                int totalForum = Soma(acoes, "mod_forum", "discussion", "created") + Soma(acoes, "mod_forum", "discussion", "viewed");
                int totalEntrega = Soma(acoes, "core", "user", "graded");
                int totalQuestionario = Soma(acoes, "mod_quiz", "attempt", "submitted");
                int totalAvaliacao = Soma(acoes, "mod_assign", "submission", "graded") + Soma(acoes, "mod_quiz", "attempt", "graded");

               
                // Converte essas quantidades em uma nota de 0 a 10 (limitada por um máximo esperado)
                double notaVisualizacao = NotaPorFaixa(totalVisualizacoes, 100);
                double notaForum = NotaPorFaixa(totalForum, 30);
                double notaEntrega = NotaPorFaixa(totalEntrega, 20);
                double notaQuiz = NotaPorFaixa(totalQuestionario, 2);
                double notaAvaliacao = NotaPorFaixa(totalAvaliacao, 10);

                // Aplica os pesos para gerar a nota final ponderada
                double les =
                    notaEntrega * config.PesoEntrega +
                    notaForum * config.PesoForum +
                    notaVisualizacao * config.PesoVisualizacao +
                    notaQuiz * config.PesoQuiz +
                    notaAvaliacao * config.PesoAvaliacao;

                // Escala o resultado e arredonda
                return Math.Min(100.0, Math.Round(les, 2)); //<- para n ultrapassar 100%
            }

            // Soma todas as ocorrências de uma ação específica
            private int Soma(List<AcaoQuantidade> acoes, string component, string target, string action)
            {
                return acoes
                    .Where(a =>
                        a.Component == component &&
                        a.Target == target &&
                        a.Action == action)
                    .Sum(a => a.Quantidade);
            }

            // Converte um valor bruto para uma nota entre 0 e 10, limitado por um valor máximo esperado
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
            var unwind = new BsonDocument("$unwind", "$logs");
            var matchLogs = new BsonDocument("$match", new BsonDocument("logs.user_id", userId));
            var replaceRoot = new BsonDocument("$replaceRoot", new BsonDocument("newRoot", "$logs"));
            var pipeline = new List<BsonDocument> { unwind, matchLogs, replaceRoot };

            var resultado = await _collection.AggregateAsync<BsonDocument>(pipeline);
            var raw = await resultado.ToListAsync();

            if (raw == null || raw.Count == 0)
                return "Usuário não encontrado ou sem interações.";

            var cal = CultureInfo.CurrentCulture.Calendar;

            var logs = raw
                .Select(l =>
                {
                    var dt = DateTime.Parse(l["date"].AsString);
                    var semana = cal.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                    var ano = dt.Year;

                    // Define o primeiro dia da semana (segunda-feira)
                    var inicioSemana = dt.Date.AddDays(-(int)dt.DayOfWeek + (dt.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
                    var fimSemana = inicioSemana.AddDays(6);

                    return new
                    {
                        Nome = l.GetValue("name", BsonNull.Value).ToString(),
                        Curso = l.GetValue("course_fullname", BsonNull.Value).ToString(),
                        Componente = l.GetValue("component", BsonNull.Value).ToString(),
                        Acao = l.GetValue("action", BsonNull.Value).ToString(),
                        dt,
                        semana,
                        ano,
                        inicioSemana,
                        fimSemana
                    };
                })
                .Where(x => x.Curso != null && x.Componente != null && x.Acao != null)
                .ToList();

            var nomeAluno = logs.FirstOrDefault()?.Nome ?? "Nome não disponível";
            var primeiro = logs.Min(x => x.dt);
            var ultimo = logs.Max(x => x.dt);
            var diasAtivos = logs.Select(x => x.dt.Date).Distinct().Count();
            var totInt = logs.Count;

            var resumoSemanal = logs
                .GroupBy(x => new { x.ano, x.semana, x.inicioSemana, x.fimSemana, x.Curso, x.Acao })
                .Select(g => new {
                    g.Key.ano,
                    g.Key.semana,
                    g.Key.inicioSemana,
                    g.Key.fimSemana,
                    Curso = g.Key.Curso,
                    Acao = g.Key.Acao,
                    Count = g.Count()
                })
                .GroupBy(x => new { x.ano, x.semana, x.inicioSemana, x.fimSemana })
                .OrderBy(x => x.Key.ano).ThenBy(x => x.Key.semana);

            var sb = new StringBuilder();
            sb.Append($"Nome: {nomeAluno}/ ");
            sb.Append($"1º: {primeiro:dd/MM/yyyy}/ ");
            sb.Append($"Úl.: {ultimo:dd/MM/yyyy}/ ");
            sb.Append($"DiasAtv: {diasAtivos}/ ");
            sb.Append($"TotInt: {totInt}/ ");

            foreach (var semana in resumoSemanal)
            {
                var periodo = $"{semana.Key.inicioSemana:dd/MM} à {semana.Key.fimSemana:dd/MM}";
                sb.Append($"({periodo}){{");

                foreach (var curso in semana.GroupBy(x => x.Curso).OrderBy(g => g.Key))
                {
                    var stats = curso
                        .GroupBy(x => x.Acao)
                        .OrderByDescending(g => g.Count())
                        .Select(g => $"{g.Key}:{g.Count()}");

                    sb.Append($"[{curso.Key} {string.Join(", ", stats)}]/");
                }

                sb.Append("}");
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
                $"Média de dias entre 1º e último acesso por aluno: {mediaDiasPermanencia:F2} dias "
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
