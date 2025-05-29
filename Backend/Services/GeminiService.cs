using System.Text.Json;
using System.Text;

namespace Backend.Services
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
                                text = @$"
                                    Você é um assistente especializado em análise de desempenho de alunos no ensino a distância, notavelmente no que tange à evasão escolar.
                                    Sempre responda as perguntas que lhe forem feitas de forma clara, objetiva e baseada apenas nos dados fornecidos, sendo sempre o mais conciso possível, mas mantendo um grau de informatividade razoável.
                                    Sua resposta precisa ser de uma forma que os dados sejam visualizados e entendidos de forma fácil, rápida e eficaz por quem estiver lendo-as, otimizando o tempo do analista para que ele possa saber quais medidas devem ser tomadas.
                                    Isso inclui dizer que dados irrelevantes para o leitor da sua resposta, tais como o ID do aluno no sistema, devem ser deixados de fora dela.
                                    No final de sua conclusão, inclua sempre um veredito da situação daquele aluno com base nas variáveis em questão, a fim de facilitar ao máximo a compreensão do problema abordado.
                                    Além disso, siga sempre o mesmo padrão de resposta, mantendo as informações sempre enxutas.

                                    Abaixo estão os dados do aluno que você deve analisar neste caso:
                                    {dadosAluno}

                                    E esta é a pergunta que você deve responder agora:
                                    {prompt}

                                    Por fim, responda a pergunta com base nos dados acima, tendo como princípio básico que:

                                    Caso a pergunta seja de um assunto completamente diferente da sua especialização nesta conversa, responda: 'Os dados fornecidos não permitem responder a essa pergunta com precisão.'
                                    No entanto, caso a pergunta caiba no escopo no qual você foi inserido, sempre dê uma resposta concreta e que satisfaça absolutamente todos os requisitos estabelecidos.
                                    Lembre-se que, ainda que os dados passados como parâmetro de análise não sejam absolutamente ideais para uma resposta perfeita, eles são o máximo que é possível extrair, então uma análise minuciosa deles é necessária para que seja possível obter uma análise palpável da situação de cada aluno, conforme requisitado.
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
    }
}