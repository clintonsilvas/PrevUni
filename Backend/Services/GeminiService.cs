using System.Text.Json;
using System.Text;

namespace Backend.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"]; // Lê do config.json
        }

        public async Task<string> EnviarPromptAsync(string prompt, string dadosAluno, int pos)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new
                            {
                                text = @$"
                                Você é um assistente especializado em análise de desempenho de alunos EAD, focado na evasão escolar.
                                Responda de forma clara, objetiva e concisa, baseada apenas nos dados fornecidos.
                                Otimize a resposta para que o professor entenda rápido e saiba quais medidas tomar.
                                Não repita dados técnicos, como IDs, e evite números redundantes.

                                Dados do aluno para análise:
                                {dadosAluno}

                                Pergunta atual:
                                {prompt}

                                Responda com base nos dados. Se o assunto for fora do seu escopo, diga: 'Os dados fornecidos não permitem responder a essa pergunta com precisão.'
                                Sempre entregue uma análise precisa, mesmo que os dados não sejam ideais.
                                "

                                        }
                                    }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var result = doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();

            return result;
        }

        public async Task<string> EnviarPromptCursoAsync(string prompt, string dadosCurso)
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new object[]
                {
                    new { text = $@"{dadosCurso},\n {prompt}" }
                }
            }
        }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erro na IA: {response.StatusCode} - {json}");
                return null;
            }

            using var doc = JsonDocument.Parse(json);
            var result = doc.RootElement.GetProperty("candidates")[0]
                                        .GetProperty("content")
                                        .GetProperty("parts")[0]
                                        .GetProperty("text")
                                        .GetString();

            return result;

        }

    }
}