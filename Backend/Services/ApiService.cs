// Backend/Services/ApiService.cs
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Backend.Models;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection; // Certifique-se de ter este using

namespace Backend.Services
{
    public class ApiService
    {
        private readonly MongoService _mongoService;
        private readonly IHttpClientFactory _httpClientFactory; // Mude o tipo aqui
        private string _token;

        private static ConcurrentDictionary<string, ImportacaoStatus> _importacaoStatuses = new ConcurrentDictionary<string, ImportacaoStatus>();

        // Altere o construtor para injetar IHttpClientFactory
        public ApiService(IHttpClientFactory httpClientFactory, MongoService mongoService)
        {
            _httpClientFactory = httpClientFactory;
            _mongoService = mongoService;
        }

        // Métodos GetToken, GetUsuarios, GetLogs precisam ser atualizados
        // para usar uma nova instância de HttpClient criada pela factory.

        // Métodos auxiliares para criar HttpClient
        private HttpClient CreateHttpClient()
        {
            // O nome "MoodleApiClient" é opcional, mas útil para configurações específicas
            // Se você não o configurou em Program.cs, pode usar _httpClientFactory.CreateClient()
            var client = _httpClientFactory.CreateClient("MoodleApiClient");

            // Define o timeout aqui também, caso não esteja configurado na factory
            client.Timeout = TimeSpan.FromMinutes(25); // Ajuste conforme necessário
            return client;
        }

        public async Task GetToken()
        {
            using var client = CreateHttpClient(); // Cria uma nova instância para esta operação
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                email = "hackathon@unifenas.br",
                password = "hackathon#2025"
            }), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.unifenas.br/v1/get-token", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            _token = JsonSerializer.Deserialize<TokenResponse>(json)?.access_token;
        }

        private DateTime _tokenExpiration;

        public async Task EnsureTokenAsync()
        {
            if (string.IsNullOrEmpty(_token) || DateTime.UtcNow >= _tokenExpiration)
            {
                await GetToken();
                _tokenExpiration = DateTime.UtcNow.AddMinutes(55); // exemplo, depende da validade real
            }
        }

        public async Task<List<Usuario>> GetUsuarios()
        {
            await EnsureTokenAsync(); // garante token válido antes
            using var client = CreateHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await client.GetAsync("https://api.unifenas.br/v1/moodle/usuarios");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Usuario>>(json);
        }

        public async Task<List<LogUsuario>> GetLogs(string userId)
        {
            await EnsureTokenAsync(); // garante token válido antes
            using var client = CreateHttpClient();

            var url = $"https://api.unifenas.br/v1/moodle/logs-usuario?user_id={userId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<LogUsuario>>(json);
        }
        public async Task ProcessarUsuariosInBackgroundAsync(string importacaoId)
        {
            if (!_importacaoStatuses.TryGetValue(importacaoId, out var status))
            {
                _importacaoStatuses[importacaoId] = new ImportacaoStatus { Id = importacaoId, Status = "Erro Interno", Mensagem = "Status da importação não encontrado." };
                return;
            }

            try
            {
                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Iniciando GetUsuarios().");
                var usuarios = await GetUsuarios();
                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: {usuarios.Count} usuários obtidos.");

                status.TotalUsuarios = usuarios.Count;
                status.Status = "Em Andamento";
                status.Mensagem = "Processando usuários...";

                int maxDegreeOfParallelism = 5; // Ajuste conforme sua capacidade/API
                var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);

                var tasks = usuarios.Select(async (usuario, index) =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Obtendo logs para usuário {usuario.user_id} ({index + 1} de {usuarios.Count}).");
                        var logs = await GetLogs(usuario.user_id);
                        Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Salvando logs para usuário {usuario.user_id}.");
                        await _mongoService.SalvarLogsAsync(usuario.user_id, logs);

                        status.ProgressoAtual++;
                        status.Mensagem = $"Processando logs do usuário: {usuario.user_id} ({status.ProgressoAtual} de {status.TotalUsuarios})";
                        Console.WriteLine(status.Mensagem);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }).ToList();

                await Task.WhenAll(tasks);

                status.Status = "Concluída";
                status.Mensagem = "Importação concluída com sucesso.";
                status.Fim = DateTime.UtcNow;
                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Concluída com sucesso.");
            }
            catch (Exception ex)
            {
                status.Status = "Erro";
                status.Mensagem = $"Erro durante a importação: {ex.Message}";
                status.Fim = DateTime.UtcNow;
                Console.WriteLine($"[{DateTime.Now}] Erro na importação {importacaoId}: {ex.Message}\n{ex.StackTrace}");
            }
        }


        public async Task ProcessarUsuariosSomenteComMudancaAsync(string importacaoId)
        {
            if (!_importacaoStatuses.TryGetValue(importacaoId, out var status))
            {
                _importacaoStatuses[importacaoId] = new ImportacaoStatus
                {
                    Id = importacaoId,
                    Status = "Erro Interno",
                    Mensagem = "Status da importação não encontrado."
                };
                return;
            }

            try
            {
                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Iniciando...");

                var usuariosApi = await GetUsuarios();
                var usuariosMongo = await _mongoService.GetUsuariosComUltimoAcessoAsync();

                var usuariosParaImportar = usuariosApi
                    .Where(apiUser =>
                    {
                        var mongoUser = usuariosMongo.FirstOrDefault(m => m.user_id == apiUser.user_id);
                        return mongoUser == null || mongoUser.user_lastaccess != apiUser.user_lastaccess;
                    })
                    .ToList();

                Console.WriteLine($"[{DateTime.Now}] {usuariosParaImportar.Count} usuários com alteração encontrados.");

                status.TotalUsuarios = usuariosParaImportar.Count;
                status.Status = "Em Andamento";
                status.Mensagem = "Processando usuários...";

                int maxParallel = 2;
                var semaphore = new SemaphoreSlim(maxParallel);

                var tasks = usuariosParaImportar.Select(async (usuario, index) =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        Console.WriteLine($"[{DateTime.Now}] Processando usuário {usuario.user_id} ({index + 1}/{usuariosParaImportar.Count})");
                        var logs = await GetLogs(usuario.user_id);
                        await _mongoService.SalvarLogsAsync(usuario.user_id, logs);

                        status.ProgressoAtual++;
                        status.Mensagem = $"Processando usuário {usuario.user_id} ({status.ProgressoAtual}/{status.TotalUsuarios})";
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }).ToList();

                await Task.WhenAll(tasks);

                status.Status = "Concluída";
                status.Mensagem = "Importação concluída com sucesso.";
                status.Fim = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                status.Status = "Erro";
                status.Mensagem = $"Erro: {ex.Message}";
                status.Fim = DateTime.UtcNow;
            }
        }



        public ImportacaoStatus IniciarNovaImportacao()
        {
            var importacaoId = Guid.NewGuid().ToString();
            var status = new ImportacaoStatus
            {
                Id = importacaoId,
                Status = "Iniciada",
                Mensagem = "Importação iniciada com sucesso. Aguardando processamento...",
                ProgressoAtual = 0,
                TotalUsuarios = 0,
                Inicio = DateTime.UtcNow
            };
            _importacaoStatuses[importacaoId] = status;
            return status;
        }

        public ImportacaoStatus? GetImportacaoStatus(string importacaoId)
        {
            _importacaoStatuses.TryGetValue(importacaoId, out var status);
            return status;
        }



    }
}