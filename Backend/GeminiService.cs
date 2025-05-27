using System.Text.Json;
using System.Text;

namespace Backend
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"];
        }

        public async Task<string> EnviarPromptAsync(string prompt, string dadosAluno)
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
                    //text = @$"
                    //        Você é um assistente especializado em análise de desempenho de alunos no ensino a distância. Sempre responda de forma clara, objetiva e baseada apenas nos dados fornecidos.

                    //        Abaixo estão os dados do aluno:
                    //        {dadosAluno}

                    //        Pergunta:
                    //        {prompt}

                    //        Responda com base nos dados acima. Caso a pergunta não tenha relação com os dados fornecidos, responda: 'Os dados fornecidos não permitem responder a essa pergunta com precisão.'"
                                            
                text=$@"{dadosAluno},\n {prompt}"}
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
    }
}
