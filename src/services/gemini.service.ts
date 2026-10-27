export interface GeminiRequest {
  prompt: string;
  contexto?: string | object;
  schema?: object;
}

export interface GeminiResponse<T = unknown> {
  sucesso: boolean;
  dados?: T;
  erro?: string;
}

export class GeminiService {
  private KEY = process.env.GEMINI_API_KEY!;
  private URL = process.env.GEMINI_URL!;

  async perguntar<T>( 
    req: GeminiRequest
  ): Promise<GeminiResponse<T>> {
    try {
      const response = await fetch(
        `${this.URL}?key=${this.KEY}`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify({
            contents: [
              {
                parts: [
                  {
                    text: `Instruções: ${req.prompt}

          Contexto:
          ${typeof req.contexto === "string"
            ? req.contexto
            : JSON.stringify(req.contexto ?? {})}`,
                  },
                ],
              },
            ],
            generationConfig: {
              responseMimeType: "application/json",
            },
          }),
        }
      );

      if (!response.ok) {
        const detalhe = await response.text();

        if (response.status === 429 || response.status === 503) {
          throw new Error(
            "O Gemini está sobrecarregado. Tente novamente em alguns instantes."
          );
        }

        throw new Error(`Gemini ${response.status}: ${detalhe}`);
      }

      const resultado = await response.json();

      const texto =
        resultado.candidates?.[0]?.content?.parts?.[0]?.text;

      if (!texto) {
        throw new Error("Gemini não retornou uma resposta válida.");
      }

      const jsonLimpo = texto
        .replace(/^```json\s*/i, "")
        .replace(/^```\s*/, "")
        .replace(/\s*```$/, "")
        .trim();

      const dados = JSON.parse(jsonLimpo) as T;

      return {
        sucesso: true,
        dados,
      };
    } catch (error) {
      return {
        sucesso: false,
        erro:
          error instanceof Error
            ? error.message
            : "Erro desconhecido",
      };
    }
  }
}