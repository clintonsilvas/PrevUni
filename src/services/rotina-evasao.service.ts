import { FirestoreService } from "@/services/firestore.service";
import { EvasaoService } from "@/services/evasao.service";

import {
  Instituicao,
  Aluno,
  Curso,
  Disciplina,
  Matricula,
  DadosEvasao,
} from "@/interfaces/types";

export async function executarRotinaEvasao() {
  try {
    const firestore = new FirestoreService();
    const evasaoService = new EvasaoService();

    // 1. Buscar todas as instituições
    const instituicoes =
      await firestore.listar<Instituicao>(
        "instituicoes"
      );

    let totalAlunos = 0;
    let totalPrevisoes = 0;

    // 2. Processar cada instituição
    for (const instituicao of instituicoes) {
      const base = `instituicoes/${instituicao.id}`;

      const [
        alunos,
        cursos,
        disciplinas,
        matriculas,
      ] = await Promise.all([
        firestore.listar<Aluno>(
          `${base}/alunos`
        ),
        firestore.listar<Curso>(
          `${base}/cursos`
        ),
        firestore.listar<Disciplina>(
          `${base}/disciplinas`
        ),
        firestore.listar<Matricula>(
          `${base}/matriculas`
        ),
      ]);

      // 3. Calcular dados dos alunos
      const dadosEvasao: DadosEvasao[] = [];

      for (const aluno of alunos) {
        const curso = cursos.find(
          (curso) =>
            curso.id === aluno.cursoId
        );

        if (!curso) {
          continue;
        }

        const dados =
          await evasaoService.calcularDadosAluno(
            aluno,
            curso,
            disciplinas,
            matriculas
          );

        dadosEvasao.push(dados);
      }

      if (dadosEvasao.length === 0) {
        continue;
      }

      // 4. Enviar dados para a RNA
      const previsoes =
        await evasaoService.preverEvasao(
          instituicao.id,
          dadosEvasao
        );

      totalAlunos += alunos.length;
      totalPrevisoes += previsoes.length;
    }

    const resultado = {
      sucesso: true,
      totalAlunos,
      totalPrevisoes,
      data: new Date(),
    };

    // 5. Resultado
    console.log("Rotina de evasão finalizada:", resultado);
    return resultado;
  } catch (error) {
    console.error(
      "Erro na rotina de previsão de evasão:",
      error
    );

    throw error;
  }
}