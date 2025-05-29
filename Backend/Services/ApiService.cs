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

        public async Task<List<Usuario>> GetUsuarios()
        {
            await GetToken(); // GetToken já cria e usa seu próprio HttpClient temporário
            using var client = CreateHttpClient(); // Cria uma nova instância para esta operação
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await client.GetAsync("https://api.unifenas.br/v1/moodle/usuarios");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Usuario>>(json);
        }

        public async Task<List<LogUsuario>> GetLogs(string userId)
        {
            await GetToken(); // GetToken já cria e usa seu próprio HttpClient temporário
            using var client = CreateHttpClient(); // Cria uma nova instância para esta operação
            var url = $"https://api.unifenas.br/v1/moodle/logs-usuario?user_id={userId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {_token}");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<LogUsuario>>(json);
        }

        public async Task ProcessarUsuariosInBackgroundAsync(string importacaoId)
        {
            if (!_importacaoStatuses.TryGetValue(importacaoId, out var status))
            {
                status = new ImportacaoStatus { Id = importacaoId, Status = "Erro Interno", Mensagem = "Status da importação não encontrado." };
                _importacaoStatuses[importacaoId] = status;
                return;
            }

            try
            {
                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Iniciando GetUsuarios().");
                var usuarios = await GetUsuarios(); // GetUsuarios já está usando a factory
                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: {usuarios.Count} usuários obtidos.");

                status.TotalUsuarios = usuarios.Count;
                status.Status = "Em Andamento";
                status.Mensagem = "Processando usuários...";

                for (int i = 0; i < usuarios.Count; i++)
                {
                    var usuario = usuarios[i];
                    Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Obtendo logs para usuário {usuario.user_id} ({i + 1} de {usuarios.Count}).");
                    var logs = await GetLogs(usuario.user_id); // GetLogs já está usando a factory
                    Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Salvando logs para usuário {usuario.user_id}.");
                    await _mongoService.SalvarLogsAsync(usuario.user_id, logs);

                    status.ProgressoAtual = i + 1;
                    status.Mensagem = $"Processando logs do usuário: {usuario.user_id} ({status.ProgressoAtual} de {status.TotalUsuarios})";
                    Console.WriteLine(status.Mensagem);
                }

                status.Status = "Concluída";
                status.Mensagem = "Importação concluída com sucesso.";
                status.Fim = DateTime.UtcNow;
                Console.WriteLine($"[{DateTime.Now}] Importação {importacaoId}: Concluída com sucesso.");
            }
            catch (HttpRequestException httpEx)
            {
                status.Status = "Erro";
                status.Mensagem = $"Erro de HTTP durante a importação: {httpEx.Message}. Status Code: {httpEx.StatusCode}";
                status.Fim = DateTime.UtcNow;
                Console.WriteLine($"[{DateTime.Now}] Erro HTTP na importação {importacaoId}: {httpEx.Message}\n{httpEx.StackTrace}");
            }
            catch (TaskCanceledException tce)
            {
                status.Status = "Erro";
                status.Mensagem = $"Importação cancelada (timeout ou interrupção): {tce.Message}";
                status.Fim = DateTime.UtcNow;
                Console.WriteLine($"[{DateTime.Now}] Task Cancelled na importação {importacaoId}: {tce.Message}\n{tce.StackTrace}");
            }
            catch (Exception ex)
            {
                status.Status = "Erro";
                status.Mensagem = $"Erro geral durante a importação: {ex.Message}";
                status.Fim = DateTime.UtcNow;
                Console.WriteLine($"[{DateTime.Now}] Erro geral na importação {importacaoId}: {ex.Message}\n{ex.StackTrace}");
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