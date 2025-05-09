using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Backend.Models;

namespace Backend
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string _token;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task GetToken()
        {
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                email = "hackathon@unifenas.br",
                password = "hackathon#2025"
            }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.unifenas.br/v1/get-token", content);
            var json = await response.Content.ReadAsStringAsync();
            _token = JsonSerializer.Deserialize<TokenResponse>(json)?.access_token;
        }

        public async Task<List<Usuario>> GetUsuarios()
        {
            await GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            var response = await _httpClient.GetAsync("https://api.unifenas.br/v1/moodle/usuarios");
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Usuario>>(json);
        }
        public async Task<List<LogUsuario>> GetLogs(string userId)
        {
            await GetToken();
            var url = $"https://api.unifenas.br/v1/moodle/logs-usuario?user_id={userId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Authorization", $"Bearer {_token}");
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<LogUsuario>>(json);
        }


    }

}
