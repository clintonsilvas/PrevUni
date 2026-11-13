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
    const instituicoes = await firestore.listar<Instituicao>("instituicoes");

    let totalAlunos = 0;
    let totalPrevisoes = 0;
    let totalSemCurso = 0;
    let totalSemMatricula = 0;

    // 2. Processar cada instituição
    for (const instituicao of instituicoes) {
      const base = `instituicoes/${instituicao.id}`;

      const [alunos, cursos, disciplinas, matriculas] = await Promise.all([
        firestore.listar<Aluno>(`${base}/alunos`),
        firestore.listar<Curso>(`${base}/cursos`),
        firestore.listar<Disciplina>(`${base}/disciplinas`),
        firestore.listar<Matricula>(`${base}/matriculas`),
      ]);

      console.log(
        `[${instituicao.id}] alunos=${alunos.length} cursos=${cursos.length} ` +
        `disciplinas=${disciplinas.length} matriculas=${matriculas.length}`
      );

      // Índices para evitar find/filter repetidos e comparar tudo como string
      const cursoPorId = new Map(cursos.map((c) => [String(c.id), c]));
      const alunosComMatricula = new Set(
        matriculas.map((m) => String(m.alunoId))
      );

      // 3. Calcular dados dos alunos
      const dadosEvasao: DadosEvasao[] = [];
      const semCurso: string[] = [];
      const semMatricula: string[] = [];

      for (const aluno of alunos) {
        // CORREÇÃO: não descarta mais o aluno quando o curso não é encontrado
        const curso = cursoPorId.get(String(aluno.cursoId)) ?? null;

        if (!curso) semCurso.push(aluno.id);
        if (!alunosComMatricula.has(String(aluno.id))) {
          semMatricula.push(aluno.id);
        }

        const dados = await evasaoService.calcularDadosAluno(
          aluno,
          curso,
          disciplinas,
          matriculas
        );

        dadosEvasao.push(dados);
      }

      if (semCurso.length > 0) {
        console.warn(
          `[${instituicao.id}] ${semCurso.length} alunos sem curso correspondente. ` +
          `Exemplos: ${semCurso.slice(0, 5).join(", ")}`
        );
      }
      if (semMatricula.length > 0) {
        console.warn(
          `[${instituicao.id}] ${semMatricula.length} alunos sem nenhuma matrícula. ` +
          `Exemplos: ${semMatricula.slice(0, 5).join(", ")}`
        );
      }

      totalSemCurso += semCurso.length;
      totalSemMatricula += semMatricula.length;

      if (dadosEvasao.length === 0) {
        continue;
      }

      // 4. Enviar dados para a RNA
      const previsoes = await evasaoService.preverEvasao(
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
      totalSemCurso,
      totalSemMatricula,
      data: new Date(),
    };

    // 5. Resultado
    console.log("Rotina de evasão finalizada:", resultado);
    return resultado;
  } catch (error) {
    console.error("Erro na rotina de previsão de evasão:", error);
    throw error;
  }
}