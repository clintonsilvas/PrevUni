
import { onSchedule } from "firebase-functions/v2/scheduler";
import { logger } from "firebase-functions";

export const preverEvasaoDiariamente = onSchedule(
  {
    schedule: "15 22 * * *",
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

export const sincronizarInstituicoesValidas= onSchedule(
  {
    schedule: "15 22 * * *",
    timeZone: "America/Sao_Paulo",    
    timeoutSeconds: 540,
  },
  async () => {
    const url = "https://prevuni.app.br/api/sync";

    const resposta = await fetch(url, {
      method: "POST"      
    });

    if (!resposta.ok) {
      const erro = await resposta.text();
      logger.error("Falha na rotina de sincronização", erro);
      throw new Error(`Erro HTTP ${resposta.status}`);
    }

    const resultado = await resposta.json();
    logger.info("Rotina de sincronização executada", resultado);
  }
);