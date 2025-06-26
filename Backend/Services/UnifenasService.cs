// Backend/Services/ApiService.cs
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Backend.Models;

namespace Backend.Services
{
    public class UnifenasService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private string _token;
        private DateTime _tokenExpiration;

        public UnifenasService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateHttpClient()
        {
            var client = _httpClientFactory.CreateClient("MoodleApiClient");
            client.Timeout = TimeSpan.FromMinutes(25);
            return client;
        }

        private async Task GetTokenAsync()
        {
            using var client = CreateHttpClient();
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                email = "hackathon@unifenas.br",
                password = "hackathon#2025"
            }), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.unifenas.br/v1/get-token", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            _token = JsonSerializer.Deserialize<TokenResponse>(json)?.access_token;
            _tokenExpiration = DateTime.UtcNow.AddMinutes(55);
        }

        private async Task EnsureTokenAsync()
        {
            if (string.IsNullOrEmpty(_token) || DateTime.UtcNow >= _tokenExpiration)
                await GetTokenAsync();
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            await EnsureTokenAsync();
            using var client = CreateHttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await client.GetAsync("https://api.unifenas.br/v1/moodle/usuarios");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Usuario>>(json);
        }

        public async Task<List<LogUsuario>> GetLogsAsync(string userId)
        {
            await EnsureTokenAsync();
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
    }
}
