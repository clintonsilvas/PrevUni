
import { onSchedule } from "firebase-functions/v2/scheduler";
import { logger } from "firebase-functions";

export const preverEvasaoDiariamente = onSchedule(
  {
    schedule: "55 16 * * *",
    timeZone: "America/Sao_Paulo",    
    timeoutSeconds: 540,
  },
  async () => {
    const url = "https://prevuni.app.br/api/prever-evasao";

    const resposta = await fetch(url, {
       method: "GET"      
    });

    if (!resposta.ok) {
      const erro = await resposta.text();
      logger.error("Falha na rotina de evasão", erro);
      throw new Error(`Erro HTTP ${resposta.status}`);
    }

    const resultado = await resposta.json();
    logger.info("Rotina de evasão executada", resultado);
  }
);
